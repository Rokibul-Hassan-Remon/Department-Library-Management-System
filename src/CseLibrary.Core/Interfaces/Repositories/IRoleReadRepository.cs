using CseLibrary.Core.DTOs;

namespace CseLibrary.Core.Interfaces.Repositories;

/// <summary>Read-only; roles are written through RoleManager.</summary>
public interface IRoleReadRepository
{
    Task<IReadOnlyList<RoleDto>> GetListAsync(CancellationToken ct = default);
    Task<RoleDto?> GetDetailsAsync(string id, CancellationToken ct = default);
}
