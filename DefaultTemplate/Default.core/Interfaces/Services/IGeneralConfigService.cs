using Default.core.DTOs;

namespace Default.core.Interfaces.Services;

public interface IGeneralConfigService
{
    Task<IEnumerable<GeneralConfigDto>> GetAllAsync();
    Task<GeneralConfigDto?> GetByIdAsync(int id);
    Task<GeneralConfigDto?> GetByKeyAsync(string key);
    Task<(bool Success, string? Error, GeneralConfigDto? Config)> CreateAsync(CreateGeneralConfigDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(UpdateGeneralConfigDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}
