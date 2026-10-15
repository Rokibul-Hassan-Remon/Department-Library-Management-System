using CseLibrary.Core.Exceptions;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Core.Results;
using CseLibrary.Data.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CseLibrary.Data.Repositories;

/// <summary>
/// Repositories are created lazily over the one scoped context (D-12: valid only while they need nothing
/// else). No IDisposable: the context belongs to DI (D-16 Q9).
/// </summary>
public class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
{
    private AuditLogRepository? _auditLogs;
    private GeneralConfigRepository? _generalConfigs;
    private PermissionRepository? _permissions;
    private RoleReadRepository? _roles;
    private RolePermissionRepository? _rolePermissions;
    private UserReadRepository? _users;

    public IAuditLogRepository AuditLogs => _auditLogs ??= new(db);
    public IGeneralConfigRepository GeneralConfigs => _generalConfigs ??= new(db);
    public IPermissionRepository Permissions => _permissions ??= new(db);
    public IRoleReadRepository Roles => _roles ??= new(db);
    public IRolePermissionRepository RolePermissions => _rolePermissions ??= new(db);
    public IUserReadRepository Users => _users ??= new(db);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (Translate(ex) is { } translated)
        {
            throw translated;
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> work,
        Func<CancellationToken, Task<bool>>? verifySucceeded = null,
        CancellationToken ct = default) where TResult : Result
    {
        if (db.Database.CurrentTransaction is not null)
            throw new InvalidOperationException("A transaction is already active; nested ExecuteInTransactionAsync is not supported.");

        TResult? committing = null; // the attempt whose commit was sent; verifySucceeded checks it landed
        try
        {
            return await db.Database.CreateExecutionStrategy().ExecuteAsync<object?, TResult>(null, async (_, _, attemptCt) =>
            {
                committing = null;
                db.ChangeTracker.Clear(); // a retry starts from fresh state (D-14)
                await using var tx = await db.Database.BeginTransactionAsync(attemptCt);
                try
                {
                    var result = await work(attemptCt);
                    if (!result.Succeeded)
                    {
                        await tx.RollbackAsync(attemptCt);
                        db.ChangeTracker.Clear();
                        return result;
                    }
                    await db.SaveChangesAsync(attemptCt);
                    committing = result;
                    await tx.CommitAsync(attemptCt);
                    return result;
                }
                catch
                {
                    db.ChangeTracker.Clear(); // disposing tx rolls back
                    throw;
                }
            }, async (_, _, verifyCt) => new ExecutionResult<TResult>(
                committing is not null && verifySucceeded is not null && await verifySucceeded(verifyCt), committing!), ct);
        }
        catch (DbUpdateException ex) when (Translate(ex) is { } translated)
        {
            throw translated;
        }
    }

    public const string AdminGuardLock = "CseLibrary.AdminGuard";

    public async Task AcquireAdminGuardLockAsync(CancellationToken ct = default)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("The admin guard lock is held by a transaction; none is active.");

        // Waits up to the command timeout; a negative result means the lock was not granted.
        var result = new SqlParameter("@result", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        await db.Database.ExecuteSqlRawAsync(
            "EXEC @result = sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction'",
            [result, new SqlParameter("@resource", AdminGuardLock)], ct);
        if ((int)result.Value < 0) throw new InvalidOperationException($"sp_getapplock returned {result.Value}.");
    }

    private static Exception? Translate(DbUpdateException ex) => ex switch
    {
        DbUpdateConcurrencyException => new ConcurrencyConflictException("The record was changed by someone else.", ex),
        { InnerException: SqlException { Number: 2601 or 2627 } } => new DuplicateKeyException("A record with the same key already exists.", ex),
        _ => null
    };
}
