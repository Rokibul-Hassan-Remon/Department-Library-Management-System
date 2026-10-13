using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

public class RoleReadRepository(ApplicationDbContext db) : IRoleReadRepository
{
    public async Task<IReadOnlyList<RoleDto>> GetListAsync(CancellationToken ct = default) =>
        await ToDto(db.Roles.OrderBy(r => r.Name)).ToListAsync(ct);

    public Task<RoleDto?> GetDetailsAsync(string id, CancellationToken ct = default) =>
        ToDto(db.Roles.Where(r => r.Id == id)).FirstOrDefaultAsync(ct);

    // Only ids, and only of permissions that are not deleted.
    private IQueryable<RoleDto> ToDto(IQueryable<ApplicationRole> q) =>
        q.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            Description = r.Description,
            CreatedAt = r.CreatedAt,
            PermissionIds = db.RolePermissions
                .Where(rp => rp.RoleId == r.Id && db.Permissions.Any(p => p.Id == rp.PermissionId))
                .Select(rp => rp.PermissionId).ToList()
        });
}
