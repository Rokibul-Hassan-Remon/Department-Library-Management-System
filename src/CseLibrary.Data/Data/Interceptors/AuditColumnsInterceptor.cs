using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CseLibrary.Data.Data.Interceptors;

/// <summary>
/// Sets the row audit columns (D-50) and turns a delete of a <see cref="BaseEntity"/> into a soft delete
/// (D-51). Services never set these values. Writes go through the entry API so only these columns are
/// marked modified, whatever the change-detection order.
/// </summary>
public sealed class AuditColumnsInterceptor(ICurrentUser currentUser, TimeProvider time) : SaveChangesInterceptor
{
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
        context.ChangeTracker.DetectChanges();
        var now = time.GetUtcNow().UtcDateTime;
        var userId = currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is BaseEntity)
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Unchanged;
                    Set(entry, nameof(BaseEntity.Status), RecordStatus.Deleted);
                }
                if (entry.State == EntityState.Added)
                {
                    Set(entry, nameof(BaseEntity.CreatedAt), now);
                    Set(entry, nameof(BaseEntity.CreatedBy), userId);
                }
                else if (entry.State == EntityState.Modified)
                {
                    Set(entry, nameof(BaseEntity.UpdatedAt), now);
                    Set(entry, nameof(BaseEntity.UpdatedBy), userId);
                }
            }
            else if (entry.Entity is ApplicationUser or ApplicationRole && entry.State == EntityState.Added)
            {
                Set(entry, "CreatedAt", now);
            }
            else if (entry.Entity is ApplicationUser && entry.State == EntityState.Modified)
            {
                Set(entry, nameof(ApplicationUser.UpdatedAt), now);
            }
        }
    }

    private static void Set(EntityEntry entry, string property, object? value) =>
        entry.Property(property).CurrentValue = value;
}
