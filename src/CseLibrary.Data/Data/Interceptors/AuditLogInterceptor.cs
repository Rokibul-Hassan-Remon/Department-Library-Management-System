using System.Diagnostics;
using System.Text.Json;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Data.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CseLibrary.Data.Data.Interceptors;

/// <summary>
/// D-21 / D-23: adds an entity audit row for every <see cref="IAuditable"/> change to the same SaveChanges, and stamps
/// actor, time and correlation (request TraceId) on every new <see cref="AuditLog"/> row, security events included.
/// Registered after <see cref="AuditColumnsInterceptor"/>, so a delete is already Status = Deleted here.
/// </summary>
public sealed class AuditLogInterceptor(ICurrentUser currentUser, TimeProvider time) : SaveChangesInterceptor
{
    private const int MaxLength = 1000;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Apply(DbContext? context)
    {
        if (context is null) return;
        var tracker = context.ChangeTracker;

        // Rows from an earlier failed SaveChanges in this scope are rebuilt, never doubled.
        foreach (var stale in tracker.Entries<AuditLog>().Where(e => e.State == EntityState.Added && e.Entity.Category == AuditCategory.Entity).ToList())
            stale.State = EntityState.Detached;

        context.AddRange(tracker.Entries()
            .Where(e => e.Entity is IAuditable && e.State is EntityState.Added or EntityState.Modified)
            .Select(ToRow).OfType<AuditLog>().ToList());

        var now = time.GetUtcNow().UtcDateTime;
        var correlationId = Activity.Current?.TraceId.ToHexString();
        foreach (var log in tracker.Entries<AuditLog>().Where(e => e.State == EntityState.Added).Select(e => e.Entity))
        {
            log.ActorUserId ??= currentUser.UserId;
            log.OccurredAtUtc = now;
            log.CorrelationId ??= correlationId;
        }
    }

    // D-21 P2: Created = audited values, Updated = changed audited values, Deleted = the soft delete with a snapshot.
    private static AuditLog? ToRow(EntityEntry entry)
    {
        var audited = entry.Properties.Where(p => AuditClassification.IsAudited(p.Metadata)).ToList();
        var deleted = entry.State == EntityState.Modified
                      && entry.Property(nameof(BaseEntity.Status)) is { IsModified: true, CurrentValue: RecordStatus.Deleted };

        var (action, changes) = entry.State == EntityState.Added
            ? ("Created", audited.ToDictionary(p => p.Metadata.Name, p => Change(null, p.CurrentValue)))
            : deleted
                ? ("Deleted", audited.ToDictionary(p => p.Metadata.Name, p => Change(p.OriginalValue, null)))
                : ("Updated", audited.Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue))
                    .ToDictionary(p => p.Metadata.Name, p => Change(p.OriginalValue, p.CurrentValue)));
        if (changes.Count == 0) return null;

        return new AuditLog
        {
            Category = AuditCategory.Entity,
            EntityType = entry.Metadata.ClrType.Name,
            EntityId = Convert.ToString(entry.Property(nameof(BaseEntity.Id)).CurrentValue, System.Globalization.CultureInfo.InvariantCulture)!,
            Action = action,
            Changes = JsonSerializer.Serialize(changes)
        };
    }

    private static Dictionary<string, object?> Change(object? oldValue, object? newValue)
    {
        var change = new Dictionary<string, object?>();
        if (oldValue is not null) change["Old"] = Safe(oldValue);
        if (newValue is not null) change["New"] = Safe(newValue);
        return change;
    }

    private static object? Safe(object value) => value switch
    {
        string s => s.Length > MaxLength ? s[..MaxLength] : s,
        Enum e => e.ToString(),
        _ => value
    };
}
