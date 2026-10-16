using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services.Services;

public class GeneralConfigService(IUnitOfWork uow, ILookupNormalizer normalizer) : IGeneralConfigService
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
        if (await CheckDefaultRoleAsync(dto.Key, dto.Value, ct) is { } invalid) return invalid;

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
        if (await CheckDefaultRoleAsync(dto.Key, dto.Value, ct) is { } invalid) return invalid;

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

    // D-56 Q-S21-1: refuse an unknown or privileged role as a registration default; empty means no role.
    private async Task<Result?> CheckDefaultRoleAsync(string key, string? value, CancellationToken ct) =>
        RegistrationConfigKeys.All.Contains(key.Trim(), StringComparer.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(value) && await DefaultRole.ResolveAsync(uow, normalizer, value, ct) == null
            ? Result.Failure(ErrorCodes.InvalidState, $"\"{value}\" is not a role, or it has permissions other than {string.Join(", ", Permissions.MemberAllowlist)}.")
            : null;

    private static Result KeyExists() => Result.Failure(ErrorCodes.Duplicate, "Key already exists");
    private static Result NotFound() => Result.Failure(ErrorCodes.NotFound, "Config not found");
}
