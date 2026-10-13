using CseLibrary.Core.Exceptions;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Core.Results;
using CseLibrary.Data.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

/// <summary>
/// Repositories are created lazily over the one scoped context (D-12: valid only while they need nothing
/// else). No IDisposable: the context belongs to DI (D-16 Q9).
/// </summary>
public class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
{
    private GeneralConfigRepository? _generalConfigs;
    private PermissionRepository? _permissions;
    private RoleReadRepository? _roles;
    private RolePermissionRepository? _rolePermissions;
    private UserReadRepository? _users;

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
        Func<CancellationToken, Task<TResult>> work, CancellationToken ct = default) where TResult : Result
    {
        if (db.Database.CurrentTransaction is not null)
            throw new InvalidOperationException("A transaction is already active; nested ExecuteInTransactionAsync is not supported.");

        try
        {
            return await db.Database.CreateExecutionStrategy().ExecuteAsync(async attemptCt =>
            {
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
                    await tx.CommitAsync(attemptCt);
                    return result;
                }
                catch
                {
                    db.ChangeTracker.Clear(); // disposing tx rolls back
                    throw;
                }
            }, ct);
        }
        catch (DbUpdateException ex) when (Translate(ex) is { } translated)
        {
            throw translated;
        }
    }

    private static Exception? Translate(DbUpdateException ex) => ex switch
    {
        DbUpdateConcurrencyException => new ConcurrencyConflictException("The record was changed by someone else.", ex),
        { InnerException: SqlException { Number: 2601 or 2627 } } => new DuplicateKeyException("A record with the same key already exists.", ex),
        _ => null
    };
}
