using CseLibrary.Core.DTOs;

namespace CseLibrary.Core.Interfaces.Services;

public interface IPermissionService
{
    Task<IEnumerable<PermissionDto>> GetAllAsync();
    Task<PermissionDto?> GetByIdAsync(int id);
    Task<(bool Success, string? Error, PermissionDto? Permission)> CreateAsync(CreatePermissionDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(UpdatePermissionDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
    Task<IEnumerable<string>> GetPermissionsForUserAsync(string userId);
}
