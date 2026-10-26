using CseLibrary.Core.DTOs;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Core.Interfaces.Services;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPageAsync(PageRequest page, CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Result> CreateAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(UpdateUserDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(string id, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(string id, string newPassword, CancellationToken ct = default);
    Task<Result> ChangeOwnPasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default);
    /// <summary>Stores a new photo (JPG/PNG/WebP, max 2 MB) or, with null, removes it; the replaced file is deleted.</summary>
    Task<Result> SetPhotoAsync(string userId, ImageUpload? image, CancellationToken ct = default);
}
