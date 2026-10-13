using AutoMapper;
using Default.core.DTOs;
using Default.core.Entities;
using Default.core.Interfaces;
using Default.core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Default.service.Services;

public class GeneralConfigService : IGeneralConfigService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GeneralConfigService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GeneralConfigDto>> GetAllAsync()
    {
        var list = await _uow.GeneralConfigs.Query().AsNoTracking().OrderBy(c => c.Group).ThenBy(c => c.Key).ToListAsync();
        return list.Select(c => _mapper.Map<GeneralConfigDto>(c));
    }

    public async Task<GeneralConfigDto?> GetByIdAsync(int id)
    {
        var entity = await _uow.GeneralConfigs.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<GeneralConfigDto>(entity);
    }

    public async Task<GeneralConfigDto?> GetByKeyAsync(string key)
    {
        var entity = await _uow.GeneralConfigs.Query().AsNoTracking().FirstOrDefaultAsync(c => c.Key == key);
        return entity == null ? null : _mapper.Map<GeneralConfigDto>(entity);
    }

    public async Task<(bool Success, string? Error, GeneralConfigDto? Config)> CreateAsync(CreateGeneralConfigDto dto)
    {
        if (await _uow.GeneralConfigs.AnyAsync(c => c.Key == dto.Key))
            return (false, "Key already exists", null);

        var entity = _mapper.Map<GeneralConfig>(dto);
        await _uow.GeneralConfigs.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return (true, null, _mapper.Map<GeneralConfigDto>(entity));
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
