using CseLibrary.Core.DTOs;
using CseLibrary.Core.Results;

namespace CseLibrary.Core.Interfaces.Services;

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken ct = default);
    Task<PermissionDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result> CreateAsync(CreatePermissionDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(UpdatePermissionDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionsForUserAsync(string userId, CancellationToken ct = default);
}
