using System.Net;
using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Data.Data;
using CseLibrary.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

/// <summary>Step 0.11, security events (D-24, D-27, Session 22 package P4): tests C4, C5.</summary>
[Collection(AppCollection.Name)]
public class SecurityEventTests(AppFactory app)
{
    private const string UserPassword = "Passw0rd!12";

    [Fact] // C4: admin actions write their events in the same transaction, by the acting Admin
    public async Task Role_and_user_changes_write_security_events()
    {
        var permissionId = (await app.WithServiceAsync(sp => sp.GetRequiredService<IPermissionService>().GetAllAsync()))
            .Single(p => p.Name == Permissions.GeneralConfig.View).Id;
        var roleName = "Role_" + Guid.NewGuid().ToString("N")[..8];
        var email = $"sec_{Guid.NewGuid():N}@test.local";

        var (roleId, userId) = await app.WithServiceAsync(async sp =>
        {
            var roles = sp.GetRequiredService<IRoleService>();
            var users = sp.GetRequiredService<IUserService>();
            var role = await roles.CreateAsync(new CreateRoleDto { Name = roleName });
            Assert.True((await roles.UpdateAsync(new UpdateRoleDto { Id = role.Value!, Name = roleName, Description = "d", PermissionIds = [permissionId] })).Succeeded);

            Assert.True((await users.CreateAsync(new CreateUserDto { UserName = email, Email = email, Password = UserPassword })).Succeeded);
            var id = (await sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email))!.Id;
            UpdateUserDto Edit(bool active, IList<string> userRoles) => new() { Id = id, Email = email, FullName = "New Name", IsActive = active, Roles = userRoles };
            Assert.True((await users.UpdateAsync(Edit(true, [roleName]))).Succeeded);
            Assert.True((await users.UpdateAsync(Edit(false, [roleName]))).Succeeded);
            Assert.True((await users.UpdateAsync(Edit(true, [roleName]))).Succeeded);
            Assert.True((await users.ResetPasswordAsync(id, "Reset-Pw-123")).Succeeded);
            Assert.True((await users.DeleteAsync(id)).Succeeded);
            return (role.Value!, id);
        });

        var roleEvents = await Events(SecurityEvent("Role"), roleId);
        Assert.Equal(["RoleCreated", "RoleUpdated", "RolePermissionsChanged"], roleEvents.Select(e => e.Action));
        Assert.Contains($"\"Added\":[{permissionId}]", roleEvents[2].Changes);

        var userEvents = await Events(SecurityEvent("User"), userId);
        Assert.Equal(["UserCreated", "UserUpdated", "UserRolesChanged", "UserDeactivated", "UserActivated", "PasswordResetByAdmin", "UserDeleted"],
            userEvents.Select(e => e.Action));
        Assert.Equal("{\"Fields\":[\"FullName\"]}", userEvents[1].Changes); // names only, never values
        Assert.Contains(roleName, userEvents[2].Changes);
        Assert.All(roleEvents.Concat(userEvents), e => Assert.Equal(TestCurrentUser.SeedAdminId, e.ActorUserId));
        Assert.All(roleEvents.Concat(userEvents), e => Assert.DoesNotContain("Pw", e.Changes ?? ""));
    }

    [Fact] // C4 / D-27: a refused or failed change writes no event (same transaction)
    public async Task Refused_or_failed_change_writes_no_security_event()
    {
        var adminId = TestCurrentUser.SeedAdminId!;
        var before = await Events(SecurityEvent("User"), adminId);
        var admin = (await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().GetByIdAsync(adminId)))!;

        var refused = await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().UpdateAsync(new UpdateUserDto
        {
            Id = adminId, Email = admin.Email, FullName = "Changed", IsActive = false, Roles = admin.Roles // last Admin
        }));
        Assert.False(refused.Succeeded);
        await Assert.ThrowsAnyAsync<InvalidOperationException>(() => app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>()
            .UpdateAsync(new UpdateUserDto { Id = adminId, Email = admin.Email, FullName = "Changed", IsActive = true, Roles = [.. admin.Roles, "NoSuchRole"] })));

        Assert.Equal(before.Count, (await Events(SecurityEvent("User"), adminId)).Count);
    }

    [Fact] // C5: sign-in events for a known user; an unknown email writes none
    public async Task Sign_in_events_are_written_for_known_users_only()
    {
        var email = $"sec_{Guid.NewGuid():N}@test.local";
        await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().CreateAsync(new CreateUserDto { UserName = email, Email = email, Password = UserPassword }));
        var userId = (await app.WithServiceAsync(sp => sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email)))!.Id;
        var loginFailedBefore = await Count("LoginFailed");

        await PostLogin($"nobody_{Guid.NewGuid():N}@test.local", "whatever-123");
        Assert.Equal(loginFailedBefore, await Count("LoginFailed"));

        await PostLogin(email, "wrong-password-1");
        var client = await app.LoggedInClient(email, UserPassword);
        await AppFactory.PostFormAsync(client, "/Account/ChangePassword", "/Account/Logout", new());
        for (var i = 0; i < 5; i++) await PostLogin(email, "wrong-password-1");

        var events = await Events(SecurityEvent("User"), userId);
        Assert.Equal(["UserCreated", "LoginFailed", "LoginSucceeded", "Logout", "LoginFailed", "LoginFailed", "LoginFailed", "LoginFailed", "LockedOut"],
            events.Select(e => e.Action));
        Assert.All(events.Skip(1), e => Assert.Equal(userId, e.ActorUserId)); // the user is the actor of their own sign-in
        Assert.All(events.Skip(1), e => Assert.Null(e.Changes)); // sign-in events store nothing typed
    }

    [Fact] // P4: self-service password change still works and writes PasswordChanged in its transaction
    public async Task Self_service_password_change_writes_PasswordChanged()
    {
        var email = $"sec_{Guid.NewGuid():N}@test.local";
        await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().CreateAsync(new CreateUserDto { UserName = email, Email = email, Password = UserPassword }));
        var userId = (await app.WithServiceAsync(sp => sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email)))!.Id;
        var client = await app.LoggedInClient(email, UserPassword);

        var wrong = await AppFactory.PostFormAsync(client, "/Account/ChangePassword", "/Account/ChangePassword", new()
        {
            ["CurrentPassword"] = "not-the-password-1", ["NewPassword"] = "Self-Changed-Pw1", ["ConfirmPassword"] = "Self-Changed-Pw1"
        });
        Assert.Equal(HttpStatusCode.OK, wrong.StatusCode); // form shown again, nothing written
        var changed = await AppFactory.PostFormAsync(client, "/Account/ChangePassword", "/Account/ChangePassword", new()
        {
            ["CurrentPassword"] = UserPassword, ["NewPassword"] = "Self-Changed-Pw1", ["ConfirmPassword"] = "Self-Changed-Pw1"
        });
        Assert.Equal(HttpStatusCode.Redirect, changed.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Account/ChangePassword")).StatusCode); // this session stays signed in

        await app.LoggedInClient(email, "Self-Changed-Pw1");
        var passwordEvents = (await Events(SecurityEvent("User"), userId)).Where(e => e.Action == "PasswordChanged").ToList();
        Assert.Equal(userId, Assert.Single(passwordEvents).ActorUserId);
    }

    private static Func<AuditLog, bool> SecurityEvent(string entityType) =>
        a => a.Category == AuditCategory.Security && a.EntityType == entityType;

    private Task<List<AuditLog>> Events(Func<AuditLog, bool> filter, string entityId) =>
        app.WithServiceAsync(async sp => (await sp.GetRequiredService<ApplicationDbContext>().AuditLogs.AsNoTracking()
            .Where(a => a.EntityId == entityId).OrderBy(a => a.Id).ToListAsync()).Where(filter).ToList());

    private Task<int> Count(string action) =>
        app.WithServiceAsync(sp => sp.GetRequiredService<ApplicationDbContext>().AuditLogs.CountAsync(a => a.Action == action));

    private Task<HttpResponseMessage> PostLogin(string email, string password) =>
        AppFactory.PostFormAsync(app.NewClient(), "/Account/Login", "/Account/Login", new()
        {
            ["Email"] = email, ["Password"] = password, ["RememberMe"] = "false"
        });
}
