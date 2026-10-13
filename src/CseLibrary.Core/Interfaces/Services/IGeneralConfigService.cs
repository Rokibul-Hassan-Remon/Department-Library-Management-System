using CseLibrary.Core.DTOs;
using CseLibrary.Core.Results;

namespace CseLibrary.Core.Interfaces.Services;

public interface IGeneralConfigService
{
    Task<IReadOnlyList<GeneralConfigDto>> GetAllAsync(CancellationToken ct = default);
    Task<GeneralConfigDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<GeneralConfigDto?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<Result> CreateAsync(CreateGeneralConfigDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(UpdateGeneralConfigDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
