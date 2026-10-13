using CseLibrary.Core.Entities;

namespace CseLibrary.Core.Interfaces.Repositories;

/// <summary>Join table with a composite key, so not an <see cref="IRepository{T}"/>.</summary>
public interface IRolePermissionRepository
{
    Task<IReadOnlyList<RolePermission>> GetForRoleForUpdateAsync(string roleId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<RolePermission> items, CancellationToken ct = default);
    void RemoveRange(IEnumerable<RolePermission> items);
}
