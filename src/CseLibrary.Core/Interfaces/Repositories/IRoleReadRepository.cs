using CseLibrary.Core.DTOs;

namespace CseLibrary.Core.Interfaces.Repositories;

/// <summary>Read-only; roles are written through RoleManager.</summary>
public interface IRoleReadRepository
{
    Task<IReadOnlyList<RoleDto>> GetListAsync(CancellationToken ct = default);
    Task<RoleDto?> GetDetailsAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetNamesAsync(CancellationToken ct = default);

    /// <summary>
    /// True when a role with one of <paramref name="normalizedNames"/> holds a permission that is not deleted.
    /// Every such role is privileged until the member-permission allowlist exists (D-56 Q-S21-5, Q-S21-10).
    /// </summary>
    Task<bool> AnyPrivilegedAsync(IReadOnlyCollection<string> normalizedNames, CancellationToken ct = default);
}
