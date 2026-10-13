using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;

namespace CseLibrary.Services.Services;

public class GeneralConfigService(IUnitOfWork uow) : IGeneralConfigService
{
    public Task<IReadOnlyList<GeneralConfigDto>> GetAllAsync(CancellationToken ct = default) =>
        uow.GeneralConfigs.GetListAsync(ct);

    public Task<GeneralConfigDto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        uow.GeneralConfigs.GetDetailsAsync(id, ct);

    public Task<GeneralConfigDto?> GetByKeyAsync(string key, CancellationToken ct = default) =>
        uow.GeneralConfigs.GetByKeyAsync(key, ct);

    public async Task<Result> CreateAsync(CreateGeneralConfigDto dto, CancellationToken ct = default)
    {
        if (await uow.GeneralConfigs.KeyExistsAsync(dto.Key, null, ct)) return KeyExists();

        await uow.GeneralConfigs.AddAsync(new GeneralConfig
        {
            Key = dto.Key, Value = dto.Value, Description = dto.Description, Group = dto.Group
        }, ct);
        return await uow.SaveAsync(ct).OnDuplicate(KeyExists());
    }

    public async Task<Result> UpdateAsync(UpdateGeneralConfigDto dto, CancellationToken ct = default)
    {
        var entity = await uow.GeneralConfigs.GetForUpdateAsync(dto.Id, ct);
        if (entity == null) return NotFound();
        if (await uow.GeneralConfigs.KeyExistsAsync(dto.Key, dto.Id, ct)) return KeyExists();

        entity.Key = dto.Key;
        entity.Value = dto.Value;
        entity.Description = dto.Description;
        entity.Group = dto.Group;
        return await uow.SaveAsync(ct).OnDuplicate(KeyExists());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.GeneralConfigs.GetForUpdateAsync(id, ct);
        if (entity == null) return NotFound();

        uow.GeneralConfigs.Remove(entity);
        return await uow.SaveAsync(ct);
    }

    private static Result KeyExists() => Result.Failure(ErrorCodes.Duplicate, "Key already exists");
    private static Result NotFound() => Result.Failure(ErrorCodes.NotFound, "Config not found");
}
