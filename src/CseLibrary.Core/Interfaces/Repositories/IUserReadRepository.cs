using CseLibrary.Core.DTOs;
using CseLibrary.Core.Paging;

namespace CseLibrary.Core.Interfaces.Repositories;

/// <summary>Read-only projection of the Identity tables; user writes stay on UserManager (D-13).</summary>
public interface IUserReadRepository
{
    Task<PagedResult<UserDto>> GetPageAsync(PageRequest page, CancellationToken ct = default);
    Task<UserDto?> GetDetailsAsync(string id, CancellationToken ct = default);

    /// <summary>Effective Admin = holds the Admin role and is Active; lockout does not matter (D-56 Q-S21-7, Q-S21-11).</summary>
    Task<bool> IsEffectiveAdminAsync(string userId, CancellationToken ct = default);
    Task<int> CountEffectiveAdminsAsync(CancellationToken ct = default);
}
