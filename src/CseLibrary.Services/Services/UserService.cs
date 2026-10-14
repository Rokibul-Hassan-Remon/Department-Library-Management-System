using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services.Services;

public class UserService(UserManager<ApplicationUser> userManager, IUnitOfWork uow) : IUserService
{
    public Task<PagedResult<UserDto>> GetPageAsync(PageRequest page, CancellationToken ct = default) =>
        uow.Users.GetPageAsync(page, ct);

    public Task<UserDto?> GetByIdAsync(string id, CancellationToken ct = default) => uow.Users.GetDetailsAsync(id, ct);

    // UserManager saves on its own; the transaction makes user + roles one change (D-14).
    // After an ambiguous commit the user's existence decides, so a retry never reports its own name as taken.
    public Task<Result> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        string? userId = null;
        return uow.ExecuteInTransactionAsync(async _ =>
        {
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

    public Task<Result> UpdateAsync(UpdateUserDto dto, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async _ =>
        {
            var user = await FindLiveAsync(dto.Id);
            if (user == null) return NotFound();

            user.Email = dto.Email;
            user.FullName = dto.FullName;
            // D-32: deactivation changes the stamp (same single write), so existing cookies stop working.
            var deactivated = user.Status == RecordStatus.Active && !dto.IsActive;
            user.Status = dto.IsActive ? RecordStatus.Active : RecordStatus.Inactive;
            var update = (await (deactivated ? userManager.UpdateSecurityStampAsync(user) : userManager.UpdateAsync(user))).ToResult();
            if (!update.Succeeded) return update;

            var current = await userManager.GetRolesAsync(user);
            var toRemove = current.Except(dto.Roles).ToList();
            var toAdd = dto.Roles.Except(current).ToList();
            if (toRemove.Count > 0)
            {
                var removed = (await userManager.RemoveFromRolesAsync(user, toRemove)).ToResult();
                if (!removed.Succeeded) return removed;
            }
            return toAdd.Count > 0 ? (await userManager.AddToRolesAsync(user, toAdd)).ToResult() : Result.Success();
        }, ct: ct).OnDuplicate(Taken());

    // D-53: users are never physically deleted. The stamp update saves the status in the same write
    // and invalidates existing cookies; roles and history stay attached to the kept row.
    public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
    {
        var user = await FindLiveAsync(id);
        if (user == null) return NotFound();
        user.Status = RecordStatus.Deleted;
        return (await userManager.UpdateSecurityStampAsync(user)).ToResult();
    }

    public async Task<Result> ResetPasswordAsync(string id, string newPassword, CancellationToken ct = default)
    {
        var user = await FindLiveAsync(id);
        if (user == null) return NotFound();
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return (await userManager.ResetPasswordAsync(user, token, newPassword)).ToResult();
    }

    // A deleted user is "not found" for every write, so an edit cannot bring it back (D-53).
    private async Task<ApplicationUser?> FindLiveAsync(string id) =>
        await userManager.FindByIdAsync(id) is { Status: not RecordStatus.Deleted } user ? user : null;

    private static Result NotFound() => Result.Failure(ErrorCodes.NotFound, "User not found");
    private static Result Taken() => Result.Failure(ErrorCodes.Duplicate, "User name or email is already taken.");
}
