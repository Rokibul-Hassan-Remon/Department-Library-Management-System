using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services.Services;

/// <summary>
/// D-56 Q-S21-12, 14, 15: only an effective Admin may create, change or delete a privileged role (one with
/// permissions); the Admin role keeps every permission, cannot be renamed or deleted, and no role can take its name.
/// </summary>
public class RoleService(RoleManager<ApplicationRole> roleManager, IUnitOfWork uow, ICurrentUser currentUser) : IRoleService
{
    private const string NameTaken = "Role name is already taken.";
    private const string AdminOnly = "Only an Admin can create, change or delete a role that has permissions.";

    public Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default) => uow.Roles.GetListAsync(ct);

    public Task<RoleDto?> GetByIdAsync(string id, CancellationToken ct = default) => uow.Roles.GetDetailsAsync(id, ct);

    public Task<IReadOnlyList<string>> GetNamesAsync(CancellationToken ct = default) => uow.Roles.GetNamesAsync(ct);

    // RoleManager saves on its own; the transaction makes role + permissions one change (D-14).
    // After an ambiguous commit the role's existence decides, so a retry never reports its own name as taken.
    public Task<Result<string>> CreateAsync(CreateRoleDto dto, CancellationToken ct = default)
    {
        string? roleId = null;
        return uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var permissionIds = await uow.Permissions.GetExistingIdsAsync(dto.PermissionIds.ToList(), attemptCt);
            if (permissionIds.Count > 0 && !await CallerIsAdminAsync(attemptCt))
                return Result<string>.Failure(ErrorCodes.Forbidden, AdminOnly);

            var role = new ApplicationRole { Name = dto.Name, Description = dto.Description };
            roleId = role.Id;
            var create = (await roleManager.CreateAsync(role)).ToResult();
            if (!create.Succeeded) return Result<string>.Failure(create.ErrorCode!, create.Error!);

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

            var isAdminRole = IsAdminName(role.Name);
            if ((isAdminRole || IsAdminName(dto.Name)) && dto.Name != role.Name)
                return Result.Failure(ErrorCodes.Forbidden, "The Admin role cannot be renamed, and no role can be renamed to Admin.");

            var desired = (await uow.Permissions.GetExistingIdsAsync(dto.PermissionIds.ToList(), attemptCt)).ToHashSet();
            if (isAdminRole && desired.Count < await uow.Permissions.CountAsync(attemptCt))
                return Result.Failure(ErrorCodes.Forbidden, "The Admin role always keeps every permission.");
            if ((desired.Count > 0 || await IsPrivilegedAsync(role, attemptCt)) && !await CallerIsAdminAsync(attemptCt))
                return Result.Failure(ErrorCodes.Forbidden, AdminOnly);

            role.Name = dto.Name;
            role.Description = dto.Description;
            var update = (await roleManager.UpdateAsync(role)).ToResult();
            if (!update.Succeeded) return update;

            var current = await uow.RolePermissions.GetForRoleForUpdateAsync(role.Id, attemptCt);
            uow.RolePermissions.RemoveRange(current.Where(rp => !desired.Contains(rp.PermissionId)));
            await uow.RolePermissions.AddRangeAsync(desired.Except(current.Select(rp => rp.PermissionId))
                .Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid }), attemptCt);
            return Result.Success();
        }, ct: ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, NameTaken));

    public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null) return Result.Failure(ErrorCodes.NotFound, "Role not found");
        if (IsAdminName(role.Name)) return Result.Failure(ErrorCodes.Forbidden, "The Admin role cannot be deleted.");
        if (await IsPrivilegedAsync(role, ct) && !await CallerIsAdminAsync(ct)) return Result.Failure(ErrorCodes.Forbidden, AdminOnly);
        return (await roleManager.DeleteAsync(role)).ToResult();
    }

    private static bool IsAdminName(string? name) => string.Equals(name, SystemRoles.Admin, StringComparison.OrdinalIgnoreCase);

    private Task<bool> IsPrivilegedAsync(ApplicationRole role, CancellationToken ct) =>
        uow.Roles.AnyPrivilegedAsync([role.NormalizedName!], ct);

    private async Task<bool> CallerIsAdminAsync(CancellationToken ct) =>
        currentUser.UserId is { } callerId && await uow.Users.IsEffectiveAdminAsync(callerId, ct);
}
