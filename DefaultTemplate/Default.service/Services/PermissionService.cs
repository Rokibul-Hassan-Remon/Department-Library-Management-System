using AutoMapper;
using Default.core.DTOs;
using Default.core.Entities;
using Default.core.Interfaces;
using Default.core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Default.service.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionService(IUnitOfWork uow, IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllAsync()
    {
        var items = await _uow.Permissions.Query().AsNoTracking().OrderBy(p => p.Module).ThenBy(p => p.Name).ToListAsync();
        return items.Select(p => _mapper.Map<PermissionDto>(p));
    }

    public async Task<PermissionDto?> GetByIdAsync(int id)
    {
        var item = await _uow.Permissions.GetByIdAsync(id);
        return item == null ? null : _mapper.Map<PermissionDto>(item);
    }

    public async Task<(bool Success, string? Error, PermissionDto? Permission)> CreateAsync(CreatePermissionDto dto)
    {
        if (await _uow.Permissions.AnyAsync(p => p.Name == dto.Name))
            return (false, "Permission name already exists", null);

        var entity = _mapper.Map<Permission>(dto);
        await _uow.Permissions.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return (true, null, _mapper.Map<PermissionDto>(entity));
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UpdatePermissionDto dto)
    {
        var entity = await _uow.Permissions.GetByIdAsync(dto.Id);
        if (entity == null) return (false, "Permission not found");

        if (entity.Name != dto.Name && await _uow.Permissions.AnyAsync(p => p.Name == dto.Name))
            return (false, "Permission name already exists");

        entity.Name = dto.Name;
        entity.DisplayName = dto.DisplayName;
        entity.Module = dto.Module;
        entity.Description = dto.Description;
        entity.UpdatedAt = DateTime.UtcNow;
        _uow.Permissions.Update(entity);
        await _uow.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var entity = await _uow.Permissions.GetByIdAsync(id);
        if (entity == null) return (false, "Permission not found");
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _uow.Permissions.Update(entity);
        await _uow.SaveChangesAsync();
        return (true, null);
    }

    public async Task<IEnumerable<string>> GetPermissionsForUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Enumerable.Empty<string>();
        var roleNames = await _userManager.GetRolesAsync(user);
        if (roleNames.Count == 0) return Enumerable.Empty<string>();

        return await _uow.RolePermissions.Query()
            .Where(rp => roleNames.Contains(rp.Role.Name!))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();
    }
}
