using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services.Services;

public class RoleService(RoleManager<ApplicationRole> roleManager, IUnitOfWork uow) : IRoleService
{
    private const string NameTaken = "Role name is already taken.";

    public Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default) => uow.Roles.GetListAsync(ct);

    public Task<RoleDto?> GetByIdAsync(string id, CancellationToken ct = default) => uow.Roles.GetDetailsAsync(id, ct);

    // RoleManager saves on its own; the transaction makes role + permissions one change (D-14).
    // After an ambiguous commit the role's existence decides, so a retry never reports its own name as taken.
    public Task<Result<string>> CreateAsync(CreateRoleDto dto, CancellationToken ct = default)
    {
        string? roleId = null;
        return uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var role = new ApplicationRole { Name = dto.Name, Description = dto.Description };
            roleId = role.Id;
            var create = (await roleManager.CreateAsync(role)).ToResult();
            if (!create.Succeeded) return Result<string>.Failure(create.ErrorCode!, create.Error!);

            var permissionIds = await uow.Permissions.GetExistingIdsAsync(dto.PermissionIds.ToList(), attemptCt);
            await uow.RolePermissions.AddRangeAsync(
                permissionIds.Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid }), attemptCt);
            return Result<string>.Success(role.Id);
        }, verifySucceeded: async c => await uow.Roles.GetDetailsAsync(roleId!, c) != null, ct: ct)
        .OnDuplicate(Result<string>.Failure(ErrorCodes.Duplicate, NameTaken));
    }

    public Task<Result> UpdateAsync(UpdateRoleDto dto, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var role = await roleManager.FindByIdAsync(dto.Id);
            if (role == null) return Result.Failure(ErrorCodes.NotFound, "Role not found");

            role.Name = dto.Name;
            role.Description = dto.Description;
            var update = (await roleManager.UpdateAsync(role)).ToResult();
            if (!update.Succeeded) return update;

            var current = await uow.RolePermissions.GetForRoleForUpdateAsync(role.Id, attemptCt);
            var desired = (await uow.Permissions.GetExistingIdsAsync(dto.PermissionIds.ToList(), attemptCt)).ToHashSet();
            uow.RolePermissions.RemoveRange(current.Where(rp => !desired.Contains(rp.PermissionId)));
            await uow.RolePermissions.AddRangeAsync(desired.Except(current.Select(rp => rp.PermissionId))
                .Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid }), attemptCt);
            return Result.Success();
        }, ct: ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, NameTaken));

    public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null) return Result.Failure(ErrorCodes.NotFound, "Role not found");
        return (await roleManager.DeleteAsync(role)).ToResult();
    }
}
