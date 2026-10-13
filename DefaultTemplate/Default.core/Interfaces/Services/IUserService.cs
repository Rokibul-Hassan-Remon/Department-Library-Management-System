using Default.core.DTOs;

namespace Default.core.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(string id);
    Task<(bool Success, string? Error, UserDto? User)> CreateAsync(CreateUserDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(UpdateUserDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(string id);
    Task<(bool Success, string? Error)> SetActiveAsync(string id, bool isActive);
    Task<(bool Success, string? Error)> ResetPasswordAsync(string id, string newPassword);
}
