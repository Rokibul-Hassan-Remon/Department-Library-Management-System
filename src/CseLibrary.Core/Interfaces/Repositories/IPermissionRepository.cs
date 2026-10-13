using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;

namespace CseLibrary.Core.Interfaces.Repositories;

public interface IPermissionRepository : IRepository<Permission>
{
    /// <summary>Small admin table: no paging (D-15 exception).</summary>
    Task<IReadOnlyList<PermissionDto>> GetListAsync(CancellationToken ct = default);
    Task<PermissionDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct = default);

    /// <summary>Distinct permission names over all of the user's roles, in one query.</summary>
    Task<IReadOnlyList<string>> GetNamesForUserAsync(string userId, CancellationToken ct = default);
}
