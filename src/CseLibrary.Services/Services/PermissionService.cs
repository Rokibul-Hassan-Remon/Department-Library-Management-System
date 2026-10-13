using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Exceptions;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;

namespace CseLibrary.Services.Services;

public class PermissionService(IUnitOfWork uow) : IPermissionService
{
    public Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken ct = default) =>
        uow.Permissions.GetListAsync(ct);

    public Task<PermissionDto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        uow.Permissions.GetDetailsAsync(id, ct);

    public async Task<Result> CreateAsync(CreatePermissionDto dto, CancellationToken ct = default)
    {
        if (await uow.Permissions.NameExistsAsync(dto.Name, null, ct)) return NameExists();

        await uow.Permissions.AddAsync(new Permission
        {
            Name = dto.Name, DisplayName = dto.DisplayName, Module = dto.Module, Description = dto.Description
        }, ct);
        return await SaveAsync(ct);
    }

    public async Task<Result> UpdateAsync(UpdatePermissionDto dto, CancellationToken ct = default)
    {
        var entity = await uow.Permissions.GetForUpdateAsync(dto.Id, ct);
        if (entity == null) return NotFound();
        if (await uow.Permissions.NameExistsAsync(dto.Name, dto.Id, ct)) return NameExists();

        entity.Name = dto.Name;
        entity.DisplayName = dto.DisplayName;
        entity.Module = dto.Module;
        entity.Description = dto.Description;
        return await SaveAsync(ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.Permissions.GetForUpdateAsync(id, ct);
        if (entity == null) return NotFound();

        uow.Permissions.Remove(entity);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public Task<IReadOnlyList<string>> GetPermissionsForUserAsync(string userId, CancellationToken ct = default) =>
        uow.Permissions.GetNamesForUserAsync(userId, ct);

    // The pre-check gives the friendly message; the filtered unique index catches a concurrent duplicate.
    private async Task<Result> SaveAsync(CancellationToken ct)
    {
        try
        {
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DuplicateKeyException)
        {
            return NameExists();
        }
    }

    private static Result NameExists() => Result.Failure(ErrorCodes.Duplicate, "Permission name already exists");
    private static Result NotFound() => Result.Failure(ErrorCodes.NotFound, "Permission not found");
}
