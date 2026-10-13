using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

public class RolePermissionRepository(ApplicationDbContext db) : IRolePermissionRepository
{
    public async Task<IReadOnlyList<RolePermission>> GetForRoleForUpdateAsync(string roleId, CancellationToken ct = default) =>
        await db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(ct);

    public Task AddRangeAsync(IEnumerable<RolePermission> items, CancellationToken ct = default) =>
        db.RolePermissions.AddRangeAsync(items, ct);

    public void RemoveRange(IEnumerable<RolePermission> items) => db.RolePermissions.RemoveRange(items);
}
