using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Services.Services;

public class GeneralConfigService : IGeneralConfigService
{
    private readonly IUnitOfWork _uow;

    public GeneralConfigService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<GeneralConfigDto>> GetAllAsync()
    {
        var list = await _uow.GeneralConfigs.Query().AsNoTracking().OrderBy(c => c.Group).ThenBy(c => c.Key).ToListAsync();
        return list.Select(c => c.ToDto());
    }

    public async Task<GeneralConfigDto?> GetByIdAsync(int id)
    {
        var entity = await _uow.GeneralConfigs.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<GeneralConfigDto?> GetByKeyAsync(string key)
    {
        var entity = await _uow.GeneralConfigs.Query().AsNoTracking().FirstOrDefaultAsync(c => c.Key == key);
        return entity?.ToDto();
    }

    public async Task<(bool Success, string? Error, GeneralConfigDto? Config)> CreateAsync(CreateGeneralConfigDto dto)
    {
        if (await _uow.GeneralConfigs.AnyAsync(c => c.Key == dto.Key))
            return (false, "Key already exists", null);

        var entity = new GeneralConfig { Key = dto.Key, Value = dto.Value, Description = dto.Description, Group = dto.Group };
        await _uow.GeneralConfigs.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return (true, null, entity.ToDto());
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UpdateGeneralConfigDto dto)
    {
        var entity = await _uow.GeneralConfigs.GetByIdAsync(dto.Id);
        if (entity == null) return (false, "Config not found");

        if (entity.Key != dto.Key && await _uow.GeneralConfigs.AnyAsync(c => c.Key == dto.Key))
            return (false, "Key already exists");

        entity.Key = dto.Key;
        entity.Value = dto.Value;
        entity.Description = dto.Description;
        entity.Group = dto.Group;
        entity.UpdatedAt = DateTime.UtcNow;
        _uow.GeneralConfigs.Update(entity);
        await _uow.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var entity = await _uow.GeneralConfigs.GetByIdAsync(id);
        if (entity == null) return (false, "Config not found");
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _uow.GeneralConfigs.Update(entity);
        await _uow.SaveChangesAsync();
        return (true, null);
    }
}
