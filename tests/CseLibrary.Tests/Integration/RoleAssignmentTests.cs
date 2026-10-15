using System.Net;
using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;
using CseLibrary.Data.Data;
using CseLibrary.Data.Repositories;
using CseLibrary.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

/// <summary>Step 0.9, D-56 Q-S21-4 … 15: privileged role assignment, last effective Admin, Admin role identity.</summary>
[Collection(AppCollection.Name)]
public class RoleAssignmentTests(AppFactory app)
{
    private const string UserPassword = "Passw0rd!12";

    [Fact] // Q-S21-4, Q-S21-8, Q-S21-10
    public async Task Non_admin_cannot_assign_or_remove_a_privileged_role()
    {
        var privileged = await CreateRole(Permissions.GeneralConfig.View);
        var plain = await CreateRole();
        var staff = await CreateUser(await CreateRole(Permissions.Users.Create, Permissions.Users.Edit));
        var target = await CreateUser(privileged);

        using (TestCurrentUser.ActAs(staff.Id))
        {
            Assert.Equal(ErrorCodes.Forbidden, (await TryCreateUser(plain, privileged)).ErrorCode);
            Assert.Equal(ErrorCodes.Forbidden, (await Update(staff, [.. staff.Roles, SystemRoles.Admin])).ErrorCode); // self-promotion
            Assert.Equal(ErrorCodes.Forbidden, (await Update(target, [])).ErrorCode);

            Assert.True((await TryCreateUser(plain)).Succeeded);
            var sameRoles = await Update(target, target.Roles, fullName: "Edited by staff");
            Assert.True(sameRoles.Succeeded, sameRoles.Error);
        }
        Assert.Equal([privileged], (await GetUser(target.Id)).Roles);
        Assert.DoesNotContain(SystemRoles.Admin, (await GetUser(staff.Id)).Roles);
    }

    [Fact] // D-57: a non-Admin cannot take over or lock out a privileged user; plain users are unchanged
    public async Task Non_admin_cannot_reset_change_email_deactivate_or_delete_a_privileged_user()
    {
        var staff = await CreateUser(await CreateRole(Permissions.Users.Edit, Permissions.Users.Delete));
        var privileged = await CreateUser(await CreateRole(Permissions.GeneralConfig.View));
        var plain = await CreateUser(await CreateRole());

        using (TestCurrentUser.ActAs(staff.Id))
        {
            Assert.Equal(ErrorCodes.Forbidden, (await ResetPassword(privileged.Id, "Changed-Pw12")).ErrorCode);
            Assert.Equal(ErrorCodes.Forbidden, (await Update(privileged, privileged.Roles, email: "x_" + privileged.Email)).ErrorCode);
            Assert.Equal(ErrorCodes.Forbidden, (await Update(privileged, privileged.Roles, isActive: false)).ErrorCode);
            Assert.Equal(ErrorCodes.Forbidden, (await Delete(privileged.Id)).ErrorCode);

            Assert.True((await ResetPassword(plain.Id, "Changed-Pw12")).Succeeded);
            Assert.True((await Update(plain, plain.Roles, email: "x_" + plain.Email)).Succeeded);
            Assert.True((await Update(plain, plain.Roles, isActive: false)).Succeeded);
            Assert.True((await Delete(plain.Id)).Succeeded);
        }
        var unchanged = await GetUser(privileged.Id);
        Assert.Equal((privileged.Email, true), (unchanged.Email, unchanged.IsActive));
        await app.LoggedInClient(privileged.Email!, UserPassword); // old password still works
    }

    [Fact] // D-57: an Admin may reset a privileged user's password; self-service password change still works
    public async Task Admin_reset_and_self_service_password_change_still_work_for_privileged_users()
    {
        var privileged = await CreateUser(await CreateRole(Permissions.GeneralConfig.View));

        Assert.True((await ResetPassword(privileged.Id, "Admin-Reset-Pw1")).Succeeded); // as the seeded Admin
        var client = await app.LoggedInClient(privileged.Email!, "Admin-Reset-Pw1");

        var changed = await AppFactory.PostFormAsync(client, "/Account/ChangePassword", "/Account/ChangePassword", new()
        {
            ["CurrentPassword"] = "Admin-Reset-Pw1", ["NewPassword"] = "Self-Changed-Pw1", ["ConfirmPassword"] = "Self-Changed-Pw1"
        });
        Assert.Equal(HttpStatusCode.Redirect, changed.StatusCode);
        await app.LoggedInClient(privileged.Email!, "Self-Changed-Pw1");
    }

    [Fact] // Q-S21-4 through the real request pipeline: the signed-in user is the caller
    public async Task Self_promotion_through_the_Users_page_is_refused()
    {
        var staff = await CreateUser(await CreateRole(Permissions.Users.View, Permissions.Users.Edit));
        var client = await app.LoggedInClient(staff.Email!, UserPassword);

        var response = await AppFactory.PostFormAsync(client, $"/Users/Edit/{staff.Id}", "/Users/Edit", new()
        {
            ["Id"] = staff.Id, ["Email"] = staff.Email!, ["IsActive"] = "true", ["SelectedRoles"] = SystemRoles.Admin
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Only an Admin can assign or remove", await response.Content.ReadAsStringAsync());
        Assert.DoesNotContain(SystemRoles.Admin, (await GetUser(staff.Id)).Roles);
    }

    [Fact] // Q-S21-7: the seeded Admin is the only effective Admin here, and acts on itself
    public async Task Last_effective_admin_cannot_lose_the_role_be_deactivated_or_deleted()
    {
        var admin = await GetUser(TestCurrentUser.SeedAdminId!);
        Assert.Equal(1, await app.WithServiceAsync(sp => sp.GetRequiredService<IUnitOfWork>().Users.CountEffectiveAdminsAsync()));

        Assert.Equal(ErrorCodes.LastAdmin, (await Update(admin, [])).ErrorCode);
        Assert.Equal(ErrorCodes.LastAdmin, (await Update(admin, admin.Roles, isActive: false)).ErrorCode);
        Assert.Equal(ErrorCodes.LastAdmin, (await Delete(admin.Id)).ErrorCode);

        var after = await GetUser(admin.Id);
        Assert.True(after.IsActive);
        Assert.Contains(SystemRoles.Admin, after.Roles);
    }

    [Fact] // Q-S21-7 with a second Admin; Q-S21-11: a locked-out Active Admin still counts
    public async Task Admin_can_be_removed_while_another_effective_admin_remains()
    {
        var second = await CreateUser(SystemRoles.Admin);
        await app.WithServiceAsync(async sp =>
        {
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            await users.SetLockoutEndDateAsync((await users.FindByIdAsync(second.Id))!, DateTimeOffset.UtcNow.AddHours(1));
            Assert.True(await sp.GetRequiredService<IUnitOfWork>().Users.IsEffectiveAdminAsync(second.Id));
        });

        var removed = await Update(second, []);
        Assert.True(removed.Succeeded, removed.Error);
        Assert.True((await Delete(second.Id)).Succeeded);
    }

    [Fact] // Q-S21-7: the check waits for the admin guard lock, so concurrent removals run one after the other
    public async Task Last_admin_check_waits_for_the_admin_guard_lock()
    {
        var second = await CreateUser(SystemRoles.Admin);
        await using var conn = new SqlConnection(app.Database.ConnectionString);
        await conn.OpenAsync();
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();
        await using (var cmd = new SqlCommand(
            "EXEC sp_getapplock @Resource = @r, @LockMode = 'Exclusive', @LockOwner = 'Transaction'", conn, tx))
        {
            cmd.Parameters.AddWithValue("@r", UnitOfWork.AdminGuardLock);
            await cmd.ExecuteNonQueryAsync();
        }

        var deactivate = Update(second, second.Roles, isActive: false);
        await Task.WhenAny(deactivate, Task.Delay(1000));
        Assert.False(deactivate.IsCompleted);

        await tx.RollbackAsync();
        var result = await deactivate;
        Assert.True(result.Succeeded, result.Error);
        Assert.True((await Delete(second.Id)).Succeeded);
    }

    [Fact] // Q-S21-12
    public async Task Non_admin_cannot_make_a_role_privileged_or_change_or_delete_a_privileged_one()
    {
        var privileged = await CreateRole(Permissions.GeneralConfig.View);
        var staff = await CreateUser(await CreateRole(Permissions.Roles.Create, Permissions.Roles.Edit, Permissions.Roles.Delete));
        var permissionId = (await PermissionIds(Permissions.GeneralConfig.View)).Single();

        using var _ = TestCurrentUser.ActAs(staff.Id);
        await app.WithServiceAsync(async sp =>
        {
            var roles = sp.GetRequiredService<IRoleService>();
            var name = "Role_" + Guid.NewGuid().ToString("N")[..8];
            Assert.Equal(ErrorCodes.Forbidden, (await roles.CreateAsync(new CreateRoleDto { Name = name, PermissionIds = [permissionId] })).ErrorCode);

            var plain = await roles.CreateAsync(new CreateRoleDto { Name = name });
            Assert.True(plain.Succeeded, plain.Error);
            Assert.Equal(ErrorCodes.Forbidden,
                (await roles.UpdateAsync(new UpdateRoleDto { Id = plain.Value!, Name = name, PermissionIds = [permissionId] })).ErrorCode);
            Assert.True((await roles.UpdateAsync(new UpdateRoleDto { Id = plain.Value!, Name = name, Description = "edited" })).Succeeded);

            var privilegedId = (await roles.GetAllAsync()).Single(r => r.Name == privileged).Id;
            Assert.Equal(ErrorCodes.Forbidden, (await roles.UpdateAsync(new UpdateRoleDto { Id = privilegedId, Name = privileged })).ErrorCode);
            Assert.Equal(ErrorCodes.Forbidden, (await roles.DeleteAsync(privilegedId)).ErrorCode);
            Assert.True((await roles.DeleteAsync(plain.Value!)).Succeeded);
        });
    }

    [Fact] // Q-S21-12, Q-S21-14, Q-S21-15 (acting as the seeded Admin)
    public Task Admin_role_keeps_its_name_and_permissions_and_cannot_be_deleted() =>
        app.WithServiceAsync(async sp =>
        {
            var roles = sp.GetRequiredService<IRoleService>();
            var admin = (await roles.GetAllAsync()).Single(r => r.Name == SystemRoles.Admin);
            UpdateRoleDto Edit(string name, IList<int> ids) =>
                new() { Id = admin.Id, Name = name, Description = admin.Description, PermissionIds = ids };

            Assert.Equal(ErrorCodes.Forbidden, (await roles.UpdateAsync(Edit("Administrators", admin.PermissionIds))).ErrorCode);
            Assert.Equal(ErrorCodes.Forbidden, (await roles.UpdateAsync(Edit(SystemRoles.Admin, admin.PermissionIds.Skip(1).ToList()))).ErrorCode);
            Assert.Equal(ErrorCodes.Forbidden, (await roles.DeleteAsync(admin.Id)).ErrorCode);
            Assert.True((await roles.UpdateAsync(Edit(SystemRoles.Admin, admin.PermissionIds))).Succeeded);

            var other = await roles.CreateAsync(new CreateRoleDto { Name = "Role_" + Guid.NewGuid().ToString("N")[..8] });
            Assert.Equal(ErrorCodes.Forbidden, (await roles.UpdateAsync(new UpdateRoleDto { Id = other.Value!, Name = "admin" })).ErrorCode);
            Assert.True((await roles.DeleteAsync(other.Value!)).Succeeded);
        });

    [Fact] // Q-S21-9, Q-S21-13: the seeder uses the fixed name; no setting is left to change it
    public async Task Seeder_uses_the_fixed_Admin_role_name()
    {
        Assert.Null(typeof(SeedAdminOptions).GetProperty("Role"));
        Assert.Contains(SystemRoles.Admin, (await GetUser(TestCurrentUser.SeedAdminId!)).Roles);
    }

    private async Task<string> CreateRole(params string[] permissions)
    {
        var name = "Role_" + Guid.NewGuid().ToString("N")[..8];
        var ids = await PermissionIds(permissions);
        var result = await app.WithServiceAsync(sp =>
            sp.GetRequiredService<IRoleService>().CreateAsync(new CreateRoleDto { Name = name, PermissionIds = ids }));
        Assert.True(result.Succeeded, result.Error);
        return name;
    }

    private async Task<UserDto> CreateUser(params string[] roles)
    {
        var email = $"ra_{Guid.NewGuid():N}@test.local";
        var result = await TryCreateUser(email, roles);
        Assert.True(result.Succeeded, result.Error);
        var id = (await app.WithServiceAsync(sp => sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email)))!.Id;
        return await GetUser(id);
    }

    private Task<Result> TryCreateUser(params string[] roles) => TryCreateUser($"ra_{Guid.NewGuid():N}@test.local", roles);

    private Task<Result> TryCreateUser(string email, string[] roles) =>
        app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().CreateAsync(new CreateUserDto
        {
            UserName = email, Email = email, Password = UserPassword, Roles = roles
        }));

    private Task<Result> Update(UserDto user, IList<string> roles, bool isActive = true, string? fullName = null, string? email = null) =>
        app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().UpdateAsync(new UpdateUserDto
        {
            Id = user.Id, Email = email ?? user.Email, FullName = fullName ?? user.FullName, IsActive = isActive, Roles = roles
        }));

    private Task<Result> ResetPassword(string id, string password) =>
        app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().ResetPasswordAsync(id, password));

    private Task<Result> Delete(string id) => app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().DeleteAsync(id));

    private async Task<UserDto> GetUser(string id) =>
        (await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().GetByIdAsync(id)))!;

    private Task<IList<int>> PermissionIds(params string[] names) =>
        app.WithServiceAsync<IList<int>>(async sp => (await sp.GetRequiredService<IPermissionService>().GetAllAsync())
            .Where(p => names.Contains(p.Name)).Select(p => p.Id).ToList());
}
