using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services.Services;

public class UserService(UserManager<ApplicationUser> userManager, IUnitOfWork uow, ICurrentUser currentUser, IImageStore images) : IUserService
{
    public Task<PagedResult<UserDto>> GetPageAsync(PageRequest page, CancellationToken ct = default) =>
        uow.Users.GetPageAsync(page, ct);

    public Task<UserDto?> GetByIdAsync(string id, CancellationToken ct = default) => uow.Users.GetDetailsAsync(id, ct);

    // UserManager saves on its own; the transaction makes user + roles one change (D-14).
    // After an ambiguous commit the user's existence decides, so a retry never reports its own name as taken.
    public Task<Result> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        string? userId = null;
        return ImageAttachment.CreateWithAsync(images, ImageKind.Users, dto.Photo, photo => uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            if (await CheckRoleChangeAsync(dto.Roles, attemptCt) is { } denied) return denied;

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName,
                EmailConfirmed = true,
                PhotoPath = photo
            };
            userId = user.Id;
            var create = (await userManager.CreateAsync(user, dto.Password)).ToResult();
            if (!create.Succeeded) return create;
            if (dto.Roles.Count > 0 && (await userManager.AddToRolesAsync(user, dto.Roles)).ToResult() is { Succeeded: false } roles) return roles;
            await AddEventAsync(user.Id, "UserCreated", new { Roles = dto.Roles }, attemptCt);
            return Result.Success();
        }, verifySucceeded: async c => await uow.Users.GetDetailsAsync(userId!, c) != null, ct: ct)
        .OnDuplicate(Taken()), ct);
    }

    // Every check runs before the first write, so a refused change leaves nothing to roll back.
    public Task<Result> UpdateAsync(UpdateUserDto dto, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var user = await FindLiveAsync(dto.Id);
            if (user == null) return NotFound();

            var current = await userManager.GetRolesAsync(user);
            var toRemove = current.Except(dto.Roles, StringComparer.OrdinalIgnoreCase).ToList();
            var toAdd = dto.Roles.Except(current, StringComparer.OrdinalIgnoreCase).ToList();
            if (await CheckRoleChangeAsync([.. toAdd, .. toRemove], attemptCt) is { } denied) return denied;

            var deactivated = user.Status == RecordStatus.Active && !dto.IsActive;
            if ((deactivated || !string.Equals(user.Email, dto.Email, StringComparison.Ordinal))
                && await CheckPrivilegedTargetAsync(current, attemptCt) is { } protectedUser) return protectedUser;
            if ((deactivated || toRemove.Contains(SystemRoles.Admin))
                && await GuardLastAdminAsync(user.Id, attemptCt) is { } lastAdmin) return lastAdmin;

            var activated = user.Status == RecordStatus.Inactive && dto.IsActive;
            var changedFields = new List<string>();
            if (user.Email != dto.Email) changedFields.Add("Email");
            if (user.FullName != dto.FullName) changedFields.Add("FullName");

            user.Email = dto.Email;
            user.FullName = dto.FullName;
            // D-32: deactivation changes the stamp (same single write), so existing cookies stop working.
            user.Status = dto.IsActive ? RecordStatus.Active : RecordStatus.Inactive;
            var update = (await (deactivated ? userManager.UpdateSecurityStampAsync(user) : userManager.UpdateAsync(user))).ToResult();
            if (!update.Succeeded) return update;

            if (toRemove.Count > 0 && (await userManager.RemoveFromRolesAsync(user, toRemove)).ToResult() is { Succeeded: false } removed) return removed;
            if (toAdd.Count > 0 && (await userManager.AddToRolesAsync(user, toAdd)).ToResult() is { Succeeded: false } added) return added;

            // D-24: in the same transaction as the change; field names only, never values.
            if (changedFields.Count > 0) await AddEventAsync(user.Id, "UserUpdated", new { Fields = changedFields }, attemptCt);
            if (deactivated) await AddEventAsync(user.Id, "UserDeactivated", null, attemptCt);
            if (activated) await AddEventAsync(user.Id, "UserActivated", null, attemptCt);
            if (toAdd.Count + toRemove.Count > 0) await AddEventAsync(user.Id, "UserRolesChanged", new { Added = toAdd, Removed = toRemove }, attemptCt);
            return Result.Success();
        }, ct: ct).OnDuplicate(Taken());

    // D-53: users are never physically deleted. The stamp update saves the status in the same write
    // and invalidates existing cookies; roles and history stay attached to the kept row.
    public Task<Result> DeleteAsync(string id, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var user = await FindLiveAsync(id);
            if (user == null) return NotFound();
            if (await CheckPrivilegedTargetAsync(await userManager.GetRolesAsync(user), attemptCt) is { } denied) return denied;
            if (await GuardLastAdminAsync(user.Id, attemptCt) is { } lastAdmin) return lastAdmin;
            user.Status = RecordStatus.Deleted;
            await AddEventAsync(user.Id, "UserDeleted", null, attemptCt); // saved by the stamp update below
            return (await userManager.UpdateSecurityStampAsync(user)).ToResult();
        }, ct: ct);

    public Task<Result> ResetPasswordAsync(string id, string newPassword, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var user = await FindLiveAsync(id);
            if (user == null) return NotFound();
            if (await CheckPrivilegedTargetAsync(await userManager.GetRolesAsync(user), attemptCt) is { } denied) return denied;
            await AddEventAsync(user.Id, "PasswordResetByAdmin", null, attemptCt);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            return (await userManager.ResetPasswordAsync(user, token, newPassword)).ToResult();
        }, ct: ct);

    // Self-service, for the signed-in user; the event shares the transaction with the change (D-24).
    public Task<Result> ChangeOwnPasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var user = await FindLiveAsync(userId);
            if (user == null) return NotFound();
            await AddEventAsync(user.Id, "PasswordChanged", null, attemptCt);
            return (await userManager.ChangePasswordAsync(user, currentPassword, newPassword)).ToResult();
        }, ct: ct);

    // Not a D-57 takeover action, so the permission check in the web layer is enough (self or Users.Edit).
    public async Task<Result> SetPhotoAsync(string userId, ImageUpload? image, CancellationToken ct = default)
    {
        var user = await FindLiveAsync(userId);
        if (user == null) return NotFound();
        return await ImageAttachment.ReplaceAsync(images, ImageKind.Users, image,
            path => { var old = user.PhotoPath; user.PhotoPath = path; return old; },
            async () => (await userManager.UpdateAsync(user)).ToResult(), ct);
    }

    private Task AddEventAsync(string userId, string action, object? details, CancellationToken ct) =>
        uow.AuditLogs.AddAsync(SecurityEvent.For(SecurityEvent.User, userId, action, details), ct);

    // D-56 Q-S21-4/8: only an effective Admin may assign or remove a privileged role (self-promotion included).
    private async Task<Result?> CheckRoleChangeAsync(ICollection<string> changed, CancellationToken ct) =>
        await IsPrivilegedAsync(changed, ct) && !await CallerIsAdminAsync(ct)
            ? Result.Failure(ErrorCodes.Forbidden, "Only an Admin can assign or remove a role with staff permissions.")
            : null;

    // D-57: only an effective Admin may reset the password of, change the email of, deactivate or delete
    // a user who holds a privileged role (account takeover otherwise bypasses D-56). Other users: unchanged.
    private async Task<Result?> CheckPrivilegedTargetAsync(ICollection<string> targetRoles, CancellationToken ct) =>
        await IsPrivilegedAsync(targetRoles, ct) && !await CallerIsAdminAsync(ct)
            ? Result.Failure(ErrorCodes.Forbidden, "Only an Admin can reset the password, change the email, deactivate or delete a user whose role has staff permissions.")
            : null;

    private async Task<bool> IsPrivilegedAsync(ICollection<string> roleNames, CancellationToken ct) =>
        roleNames.Count > 0 && await uow.Roles.AnyPrivilegedAsync(roleNames.Select(n => userManager.NormalizeName(n)!).ToList(), ct);

    private async Task<bool> CallerIsAdminAsync(CancellationToken ct) =>
        currentUser.UserId is { } callerId && await uow.Users.IsEffectiveAdminAsync(callerId, ct);

    // D-56 Q-S21-7: the lock makes this check and the caller's write one step, so two requests cannot
    // each remove one of the last two Admins. Applies to the acting user too.
    private async Task<Result?> GuardLastAdminAsync(string userId, CancellationToken ct)
    {
        await uow.AcquireAdminGuardLockAsync(ct);
        if (!await uow.Users.IsEffectiveAdminAsync(userId, ct) || await uow.Users.CountEffectiveAdminsAsync(ct) > 1) return null;
        return Result.Failure(ErrorCodes.LastAdmin, "The last active Admin cannot lose the Admin role, be deactivated or be deleted.");
    }

    // A deleted user is "not found" for every write, so an edit cannot bring it back (D-53).
    private async Task<ApplicationUser?> FindLiveAsync(string id) =>
        await userManager.FindByIdAsync(id) is { Status: not RecordStatus.Deleted } user ? user : null;

    private static Result NotFound() => Result.Failure(ErrorCodes.NotFound, "User not found");
    private static Result Taken() => Result.Failure(ErrorCodes.Duplicate, "User name or email is already taken.");
}
