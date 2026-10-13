using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Core.Paging;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

/// <summary>
/// Users list as count + one page query with roles joined in (no N+1); never selects password hash,
/// security stamp or other secrets (D-15, D-16 Q1).
/// </summary>
public class UserReadRepository(ApplicationDbContext db) : IUserReadRepository
{
    public async Task<PagedResult<UserDto>> GetPageAsync(PageRequest page, CancellationToken ct = default)
    {
        var total = await db.Users.CountAsync(ct);
        page = page.ClampTo(total);
        var items = await ToDto(db.Users.OrderBy(u => u.UserName).ThenBy(u => u.Id).Skip(page.Skip).Take(page.PageSize))
            .ToListAsync(ct);
        return new PagedResult<UserDto>(items, total, page.Page, page.PageSize);
    }

    public Task<UserDto?> GetDetailsAsync(string id, CancellationToken ct = default) =>
        ToDto(db.Users.Where(u => u.Id == id)).FirstOrDefaultAsync(ct);

    private IQueryable<UserDto> ToDto(IQueryable<ApplicationUser> q) =>
        q.Select(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            FullName = u.FullName,
            IsActive = u.Status == RecordStatus.Active,
            CreatedAt = u.CreatedAt,
            Roles = (from ur in db.UserRoles
                     join r in db.Roles on ur.RoleId equals r.Id
                     where ur.UserId == u.Id
                     orderby r.Name
                     select r.Name!).ToList()
        });
}
