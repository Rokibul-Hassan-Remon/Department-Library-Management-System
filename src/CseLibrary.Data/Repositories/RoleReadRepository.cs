using CseLibrary.Core.Constants;
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

    public async Task<IReadOnlyList<string>> GetNamesAsync(CancellationToken ct = default) =>
        await db.Roles.OrderBy(r => r.Name).Select(r => r.Name!).ToListAsync(ct);

    public Task<string?> FindNameAsync(string normalizedName, CancellationToken ct = default) =>
        db.Roles.Where(r => r.NormalizedName == normalizedName).Select(r => r.Name).FirstOrDefaultAsync(ct);

    public Task<bool> AnyPrivilegedAsync(IReadOnlyCollection<string> normalizedNames, CancellationToken ct = default) =>
        (from r in db.Roles
         where normalizedNames.Contains(r.NormalizedName!)
         join rp in db.RolePermissions on r.Id equals rp.RoleId
         join p in db.Permissions on rp.PermissionId equals p.Id // filtered: deleted permissions drop out
         where !Permissions.MemberAllowlist.Contains(p.Name)
         select r.Id).AnyAsync(ct);

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
