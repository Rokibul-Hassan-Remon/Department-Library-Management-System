using CseLibrary.Core.DTOs;
using CseLibrary.Core.Results;

namespace CseLibrary.Core.Interfaces.Services;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default);
    Task<RoleDto?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>Returns the new role's id.</summary>
    Task<Result<string>> CreateAsync(CreateRoleDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(UpdateRoleDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(string id, CancellationToken ct = default);
}
