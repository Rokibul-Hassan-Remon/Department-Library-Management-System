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
            await AddEventAsync(role.Id, "RoleCreated", new { role.Name, PermissionIds = permissionIds }, attemptCt);
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

            var changedFields = new List<string>();
            if (role.Name != dto.Name) changedFields.Add("Name");
            if (role.Description != dto.Description) changedFields.Add("Description");
            role.Name = dto.Name;
            role.Description = dto.Description;
            var update = (await roleManager.UpdateAsync(role)).ToResult();
            if (!update.Succeeded) return update;

            var current = await uow.RolePermissions.GetForRoleForUpdateAsync(role.Id, attemptCt);
            var removed = current.Where(rp => !desired.Contains(rp.PermissionId)).ToList();
            var added = desired.Except(current.Select(rp => rp.PermissionId)).ToList();
            uow.RolePermissions.RemoveRange(removed);
            await uow.RolePermissions.AddRangeAsync(added.Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid }), attemptCt);

            // D-24: in the same transaction as the change.
            if (changedFields.Count > 0) await AddEventAsync(role.Id, "RoleUpdated", new { Fields = changedFields }, attemptCt);
            if (added.Count + removed.Count > 0)
                await AddEventAsync(role.Id, "RolePermissionsChanged",
                    new { Added = added, Removed = removed.Select(rp => rp.PermissionId) }, attemptCt);
            return Result.Success();
        }, ct: ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, NameTaken));

    public Task<Result> DeleteAsync(string id, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null) return Result.Failure(ErrorCodes.NotFound, "Role not found");
            if (IsAdminName(role.Name)) return Result.Failure(ErrorCodes.Forbidden, "The Admin role cannot be deleted.");
            if (await IsPrivilegedAsync(role, attemptCt) && !await CallerIsAdminAsync(attemptCt)) return Result.Failure(ErrorCodes.Forbidden, AdminOnly);
            await AddEventAsync(role.Id, "RoleDeleted", new { role.Name }, attemptCt); // saved with the delete below
            return (await roleManager.DeleteAsync(role)).ToResult();
        }, ct: ct);

    private Task AddEventAsync(string roleId, string action, object details, CancellationToken ct) =>
        uow.AuditLogs.AddAsync(SecurityEvent.For(SecurityEvent.Role, roleId, action, details), ct);

    private static bool IsAdminName(string? name) => string.Equals(name, SystemRoles.Admin, StringComparison.OrdinalIgnoreCase);

    private Task<bool> IsPrivilegedAsync(ApplicationRole role, CancellationToken ct) =>
        uow.Roles.AnyPrivilegedAsync([role.NormalizedName!], ct);

    private async Task<bool> CallerIsAdminAsync(CancellationToken ct) =>
        currentUser.UserId is { } callerId && await uow.Users.IsEffectiveAdminAsync(callerId, ct);
}
