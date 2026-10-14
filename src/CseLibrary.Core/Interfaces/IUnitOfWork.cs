using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Core.Results;

namespace CseLibrary.Core.Interfaces;

/// <summary>
/// One scoped context behind typed repositories (D-12). Owns SaveChanges and transactions (D-14);
/// throws <see cref="Exceptions.ConcurrencyConflictException"/> / <see cref="Exceptions.DuplicateKeyException"/>.
/// </summary>
public interface IUnitOfWork
{
    IGeneralConfigRepository GeneralConfigs { get; }
    IPermissionRepository Permissions { get; }
    IRoleReadRepository Roles { get; }
    IRolePermissionRepository RolePermissions { get; }
    IUserReadRepository Users { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs <paramref name="work"/> in one transaction under the retry strategy. Pending changes are saved
    /// and committed when the result succeeds; a failed result rolls back. <paramref name="work"/> may run
    /// more than once, so it must load everything it needs itself and have no outside side effects.
    /// Nested calls throw <see cref="InvalidOperationException"/>.
    /// <paramref name="verifySucceeded"/> answers "did the last commit reach the database?" when the commit
    /// fails ambiguously; true returns that attempt's result instead of re-running <paramref name="work"/>.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> work,
        Func<CancellationToken, Task<bool>>? verifySucceeded = null,
        CancellationToken ct = default) where TResult : Result;

    /// <summary>
    /// Waits for the exclusive admin-guard lock, held until the current transaction ends, so a last-effective-Admin
    /// check and its write cannot interleave with another (D-56 Q-S21-7). Only inside <see cref="ExecuteInTransactionAsync"/>.
    /// </summary>
    Task AcquireAdminGuardLockAsync(CancellationToken ct = default);
}
