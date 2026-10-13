using Default.core.DTOs;

namespace Default.core.Interfaces.Services;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<RoleDto?> GetByIdAsync(string id);
    Task<(bool Success, string? Error, RoleDto? Role)> CreateAsync(CreateRoleDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(UpdateRoleDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(string id);
    Task<IEnumerable<string>> GetPermissionNamesForRoleAsync(string roleId);
}
