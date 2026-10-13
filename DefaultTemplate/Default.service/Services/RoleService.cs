using AutoMapper;
using Default.core.DTOs;
using Default.core.Entities;
using Default.core.Interfaces;
using Default.core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Default.service.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public RoleService(RoleManager<ApplicationRole> roleManager, IUnitOfWork uow, IMapper mapper)
    {
        _roleManager = roleManager;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        var roles = await _roleManager.Roles
            .Include(r => r.RolePermissions)
            .ToListAsync();
        return roles.Select(r => _mapper.Map<RoleDto>(r));
    }

    public async Task<RoleDto?> GetByIdAsync(string id)
    {
        var role = await _roleManager.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);
        return role == null ? null : _mapper.Map<RoleDto>(role);
    }

    public async Task<(bool Success, string? Error, RoleDto? Role)> CreateAsync(CreateRoleDto dto)
    {
        var role = new ApplicationRole { Name = dto.Name, Description = dto.Description };
        var create = await _roleManager.CreateAsync(role);
        if (!create.Succeeded)
            return (false, string.Join("; ", create.Errors.Select(e => e.Description)), null);

        if (dto.PermissionIds.Any())
        {
            foreach (var pid in dto.PermissionIds.Distinct())
                await _uow.RolePermissions.AddAsync(new RolePermission { RoleId = role.Id, PermissionId = pid });
            await _uow.SaveChangesAsync();
        }

        var roleDto = _mapper.Map<RoleDto>(role);
        roleDto.PermissionIds = dto.PermissionIds;
        return (true, null, roleDto);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UpdateRoleDto dto)
    {
        var role = await _roleManager.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == dto.Id);
        if (role == null) return (false, "Role not found");

        role.Name = dto.Name;
        role.Description = dto.Description;
        var update = await _roleManager.UpdateAsync(role);
        if (!update.Succeeded)
            return (false, string.Join("; ", update.Errors.Select(e => e.Description)));

        var currentIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
        var desiredIds = dto.PermissionIds.ToHashSet();

        var toRemove = role.RolePermissions.Where(rp => !desiredIds.Contains(rp.PermissionId)).ToList();
        var toAdd = desiredIds.Except(currentIds)
            .Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid })
            .ToList();

        if (toRemove.Any()) _uow.RolePermissions.RemoveRange(toRemove);
        if (toAdd.Any()) await _uow.RolePermissions.AddRangeAsync(toAdd);
        await _uow.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return (false, "Role not found");
        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<IEnumerable<string>> GetPermissionNamesForRoleAsync(string roleId)
    {
        return await _uow.RolePermissions.Query()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();
    }
}
