using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services.Services;

public class UserService(UserManager<ApplicationUser> userManager, IUnitOfWork uow, ICurrentUser currentUser) : IUserService
{
    public Task<PagedResult<UserDto>> GetPageAsync(PageRequest page, CancellationToken ct = default) =>
        uow.Users.GetPageAsync(page, ct);

    public Task<UserDto?> GetByIdAsync(string id, CancellationToken ct = default) => uow.Users.GetDetailsAsync(id, ct);

    // UserManager saves on its own; the transaction makes user + roles one change (D-14).
    // After an ambiguous commit the user's existence decides, so a retry never reports its own name as taken.
    public Task<Result> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        string? userId = null;
        return uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            if (await CheckRoleChangeAsync(dto.Roles, attemptCt) is { } denied) return denied;

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName,
                EmailConfirmed = true
            };
            userId = user.Id;
            var create = (await userManager.CreateAsync(user, dto.Password)).ToResult();
            if (!create.Succeeded || dto.Roles.Count == 0) return create;
            return (await userManager.AddToRolesAsync(user, dto.Roles)).ToResult();
        }, verifySucceeded: async c => await uow.Users.GetDetailsAsync(userId!, c) != null, ct: ct)
        .OnDuplicate(Taken());
    }

    // Every check runs before the first write, so a refused change leaves nothing to roll back.
    public Task<Result> UpdateAsync(UpdateUserDto dto, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var user = await FindLiveAsync(dto.Id);
            if (user == null) return NotFound();

            var current = await userManager.GetRolesAsync(user);
            var toRemove = current.Except(dto.Roles).ToList();
            var toAdd = dto.Roles.Except(current).ToList();
            if (await CheckRoleChangeAsync([.. toAdd, .. toRemove], attemptCt) is { } denied) return denied;

            var deactivated = user.Status == RecordStatus.Active && !dto.IsActive;
            if ((deactivated || toRemove.Contains(SystemRoles.Admin))
                && await GuardLastAdminAsync(user.Id, attemptCt) is { } lastAdmin) return lastAdmin;

            user.Email = dto.Email;
            user.FullName = dto.FullName;
            // D-32: deactivation changes the stamp (same single write), so existing cookies stop working.
            user.Status = dto.IsActive ? RecordStatus.Active : RecordStatus.Inactive;
            var update = (await (deactivated ? userManager.UpdateSecurityStampAsync(user) : userManager.UpdateAsync(user))).ToResult();
            if (!update.Succeeded) return update;

            if (toRemove.Count > 0)
            {
                var removed = (await userManager.RemoveFromRolesAsync(user, toRemove)).ToResult();
                if (!removed.Succeeded) return removed;
            }
            return toAdd.Count > 0 ? (await userManager.AddToRolesAsync(user, toAdd)).ToResult() : Result.Success();
        }, ct: ct).OnDuplicate(Taken());

    // D-53: users are never physically deleted. The stamp update saves the status in the same write
    // and invalidates existing cookies; roles and history stay attached to the kept row.
    public Task<Result> DeleteAsync(string id, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var user = await FindLiveAsync(id);
            if (user == null) return NotFound();
            if (await GuardLastAdminAsync(user.Id, attemptCt) is { } lastAdmin) return lastAdmin;
            user.Status = RecordStatus.Deleted;
            return (await userManager.UpdateSecurityStampAsync(user)).ToResult();
        }, ct: ct);

    public async Task<Result> ResetPasswordAsync(string id, string newPassword, CancellationToken ct = default)
    {
        var user = await FindLiveAsync(id);
        if (user == null) return NotFound();
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return (await userManager.ResetPasswordAsync(user, token, newPassword)).ToResult();
    }

    // D-56 Q-S21-4/8: only an effective Admin may assign or remove a privileged role (self-promotion included).
    private async Task<Result?> CheckRoleChangeAsync(ICollection<string> changed, CancellationToken ct)
    {
        if (changed.Count == 0) return null;
        var normalized = changed.Select(n => userManager.NormalizeName(n)!).ToList();
        if (!await uow.Roles.AnyPrivilegedAsync(normalized, ct)) return null;
        return currentUser.UserId is { } callerId && await uow.Users.IsEffectiveAdminAsync(callerId, ct)
            ? null
            : Result.Failure(ErrorCodes.Forbidden, "Only an Admin can assign or remove a role that has permissions.");
    }

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
