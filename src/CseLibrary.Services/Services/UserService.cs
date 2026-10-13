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
    public Task<Result> CreateAsync(CreateUserDto dto, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async _ =>
        {
            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName,
                EmailConfirmed = true
            };
            var create = (await userManager.CreateAsync(user, dto.Password)).ToResult();
            if (!create.Succeeded || dto.Roles.Count == 0) return create;
            return (await userManager.AddToRolesAsync(user, dto.Roles)).ToResult();
        }, ct);

    public Task<Result> UpdateAsync(UpdateUserDto dto, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async _ =>
        {
            var user = await userManager.FindByIdAsync(dto.Id);
            if (user == null) return NotFound();

            user.Email = dto.Email;
            user.FullName = dto.FullName;
            user.Status = dto.IsActive ? RecordStatus.Active : RecordStatus.Inactive;
            var update = (await userManager.UpdateAsync(user)).ToResult();
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
        }, ct);

    public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        return (await userManager.DeleteAsync(user)).ToResult();
    }

    public async Task<Result> ResetPasswordAsync(string id, string newPassword, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return (await userManager.ResetPasswordAsync(user, token, newPassword)).ToResult();
    }

    private static Result NotFound() => Result.Failure(ErrorCodes.NotFound, "User not found");
}
