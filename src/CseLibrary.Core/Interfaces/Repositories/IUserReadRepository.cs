using CseLibrary.Core.DTOs;
using CseLibrary.Core.Paging;

namespace CseLibrary.Core.Interfaces.Repositories;

/// <summary>Read-only projection of the Identity tables; user writes stay on UserManager (D-13).</summary>
public interface IUserReadRepository
{
    Task<PagedResult<UserDto>> GetPageAsync(PageRequest page, CancellationToken ct = default);
    Task<UserDto?> GetDetailsAsync(string id, CancellationToken ct = default);
}
