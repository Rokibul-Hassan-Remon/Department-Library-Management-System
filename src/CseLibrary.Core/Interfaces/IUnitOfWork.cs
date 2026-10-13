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
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> work, CancellationToken ct = default) where TResult : Result;
}
