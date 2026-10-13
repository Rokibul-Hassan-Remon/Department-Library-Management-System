using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;

namespace CseLibrary.Core.Interfaces.Repositories;

public interface IGeneralConfigRepository : IRepository<GeneralConfig>
{
    /// <summary>Small admin table: no paging (D-15 exception).</summary>
    Task<IReadOnlyList<GeneralConfigDto>> GetListAsync(CancellationToken ct = default);
    Task<GeneralConfigDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<GeneralConfigDto?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<bool> KeyExistsAsync(string key, int? excludeId, CancellationToken ct = default);
}
