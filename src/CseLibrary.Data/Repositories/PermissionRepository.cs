using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

public class PermissionRepository(ApplicationDbContext db) : Repository<Permission>(db), IPermissionRepository
{
    public async Task<IReadOnlyList<PermissionDto>> GetListAsync(CancellationToken ct = default) =>
        await ToDto(Db.Permissions.OrderBy(p => p.Module).ThenBy(p => p.Name)).ToListAsync(ct);

    public Task<PermissionDto?> GetDetailsAsync(int id, CancellationToken ct = default) =>
        ToDto(Db.Permissions.Where(p => p.Id == id)).FirstOrDefaultAsync(ct);

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct = default) =>
        Db.Permissions.AnyAsync(p => p.Name == name && p.Id != excludeId, ct);

    public async Task<IReadOnlyList<string>> GetNamesForUserAsync(string userId, CancellationToken ct = default) =>
        await (from ur in Db.UserRoles
               where ur.UserId == userId
               join rp in Db.RolePermissions on ur.RoleId equals rp.RoleId
               join p in Db.Permissions on rp.PermissionId equals p.Id // filtered: deleted permissions drop out
               select p.Name).Distinct().ToListAsync(ct);

    private static IQueryable<PermissionDto> ToDto(IQueryable<Permission> q) =>
        q.Select(p => new PermissionDto
        {
            Id = p.Id, Name = p.Name, DisplayName = p.DisplayName, Module = p.Module, Description = p.Description
        });
}
