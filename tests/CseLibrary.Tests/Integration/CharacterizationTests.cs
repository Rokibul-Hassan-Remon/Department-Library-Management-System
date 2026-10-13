using System.Net;
using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

/// <summary>
/// Characterization (step 0.5, D-43): pins the template's behaviour as it is today, defects included,
/// so Gate 2 refactoring shows exactly what changed. Tests marked "defect" are expected to flip later.
/// </summary>
[Collection(AppCollection.Name)]
public class CharacterizationTests(AppFactory app)
{
    private const string UserPassword = "Passw0rd!";

    [Fact]
    public async Task Login_with_valid_admin_credentials_redirects_home()
    {
        var response = await AppFactory.PostFormAsync(app.NewClient(), "/Account/Login", "/Account/Login", new()
        {
            ["Email"] = AppFactory.AdminEmail, ["Password"] = AppFactory.AdminPassword, ["RememberMe"] = "false"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Login_with_wrong_password_shows_generic_error()
    {
        var response = await AppFactory.PostFormAsync(app.NewClient(), "/Account/Login", "/Account/Login", new()
        {
            ["Email"] = AppFactory.AdminEmail, ["Password"] = "wrong-password", ["RememberMe"] = "false"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Invalid login attempt.", await response.Content.ReadAsStringAsync());
    }

    [Fact] // defect (D-37, test A9 in 0.9): login copies permissions into AspNetUserClaims
    public async Task Login_writes_permission_claims_to_the_database()
    {
        var (email, _) = await CreateUserWithPermissions(Permissions.GeneralConfig.View);
        await app.LoggedInClient(email, UserPassword);

        var claims = await app.WithServiceAsync(async sp =>
        {
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            return await users.GetClaimsAsync((await users.FindByEmailAsync(email))!);
        });
        Assert.Contains(claims, c => c.Type == Permissions.ClaimType && c.Value == Permissions.GeneralConfig.View);
    }

    [Fact]
    public async Task Anonymous_request_to_a_protected_page_redirects_to_login()
    {
        var response = await app.NewClient().GetAsync("/GeneralConfig");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task GeneralConfig_create_edit_delete_round_trip()
    {
        var client = await app.LoggedInClient(AppFactory.AdminEmail, AppFactory.AdminPassword);
        var key = "Test." + Guid.NewGuid().ToString("N")[..8];

        var created = await AppFactory.PostFormAsync(client, "/GeneralConfig/Create", "/GeneralConfig/Create", new()
        {
            ["Key"] = key, ["Value"] = "one", ["Group"] = "Test", ["Description"] = ""
        });
        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        Assert.Contains(key, await client.GetStringAsync("/GeneralConfig"));

        var id = (await GetConfig(key))!.Id;
        var edited = await AppFactory.PostFormAsync(client, $"/GeneralConfig/Edit/{id}", "/GeneralConfig/Edit", new()
        {
            ["Id"] = id.ToString(), ["Key"] = key, ["Value"] = "two", ["Group"] = "Test", ["Description"] = ""
        });
        Assert.Equal(HttpStatusCode.Redirect, edited.StatusCode);
        Assert.Equal("two", (await GetConfig(key))!.Value);

        var deleted = await AppFactory.PostFormAsync(client, $"/GeneralConfig/Delete/{id}", $"/GeneralConfig/Delete/{id}", new());
        Assert.Equal(HttpStatusCode.Redirect, deleted.StatusCode);
        Assert.Null(await GetConfig(key));
    }

    [Fact] // defect (D-15/D-16, fixed in 0.8): Users list issues one roles query per user (N+1)
    public async Task Users_list_shows_users_and_costs_one_extra_query_per_user()
    {
        var client = await app.LoggedInClient(AppFactory.AdminEmail, AppFactory.AdminPassword);
        await client.GetAsync("/Users"); // warm-up

        app.Commands.Reset();
        var page = await client.GetStringAsync("/Users");
        var before = app.Commands.Count;
        Assert.Contains(AppFactory.AdminEmail, page);

        for (var i = 0; i < 3; i++) await CreateUserWithPermissions();

        app.Commands.Reset();
        await client.GetStringAsync("/Users");
        // At least one per user: an exact count would flake on retries or a security-stamp check.
        Assert.True(app.Commands.Count >= before + 3, $"expected >= {before + 3} commands, got {app.Commands.Count}");
    }

    // defect (D-37, fixed in 0.9 by tests A7/A9). Login builds the cookie from AspNetUserClaims *before*
    // rewriting them, and RefreshSignInAsync is a no-op inside the login request (.NET 8+), so each
    // cookie carries the permissions written by the *previous* login. A revoked permission is denied
    // on the current cookie (DB fallback), allowed again after the next login, denied after the one after.
    [Fact]
    public async Task Revoked_permission_comes_back_for_one_login_from_stale_claim_rows()
    {
        var (email, roleId) = await CreateUserWithPermissions(Permissions.GeneralConfig.View);
        var first = await app.LoggedInClient(email, UserPassword);
        Assert.Equal(HttpStatusCode.OK, (await first.GetAsync("/GeneralConfig")).StatusCode);

        await SetRolePermissions(roleId);

        Assert.Equal(HttpStatusCode.Redirect, (await first.GetAsync("/GeneralConfig")).StatusCode);
        var second = await app.LoggedInClient(email, UserPassword);
        Assert.Equal(HttpStatusCode.OK, (await second.GetAsync("/GeneralConfig")).StatusCode);
        var third = await app.LoggedInClient(email, UserPassword);
        Assert.Equal(HttpStatusCode.Redirect, (await third.GetAsync("/GeneralConfig")).StatusCode);
    }

    [Fact] // current behaviour: a granted permission works on the next request (DB fallback in the handler)
    public async Task Granted_permission_works_on_the_next_request()
    {
        var (email, roleId) = await CreateUserWithPermissions();
        var client = await app.LoggedInClient(email, UserPassword);
        var denied = await client.GetAsync("/GeneralConfig");
        Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
        Assert.Contains("/Account/AccessDenied", denied.Headers.Location?.OriginalString);

        await SetRolePermissions(roleId, Permissions.GeneralConfig.View);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/GeneralConfig")).StatusCode);
    }

    private Task<GeneralConfigDto?> GetConfig(string key) =>
        app.WithServiceAsync(sp => sp.GetRequiredService<IGeneralConfigService>().GetByKeyAsync(key));

    private async Task<(string Email, string RoleId)> CreateUserWithPermissions(params string[] permissions)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var roleId = await app.WithServiceAsync(async sp =>
        {
            var ids = await PermissionIds(sp, permissions);
            var role = await sp.GetRequiredService<IRoleService>().CreateAsync(new CreateRoleDto { Name = "Role_" + suffix, PermissionIds = ids });
            Assert.True(role.Success, role.Error);
            return role.Role!.Id;
        });

        var email = $"user_{suffix}@test.local";
        var user = await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().CreateAsync(new CreateUserDto
        {
            UserName = email, Email = email, Password = UserPassword, Roles = ["Role_" + suffix]
        }));
        Assert.True(user.Success, user.Error);
        return (email, roleId);
    }

    private Task SetRolePermissions(string roleId, params string[] permissions) =>
        app.WithServiceAsync(async sp =>
        {
            var roles = sp.GetRequiredService<IRoleService>();
            var role = (await roles.GetByIdAsync(roleId))!;
            var result = await roles.UpdateAsync(new UpdateRoleDto
            {
                Id = role.Id, Name = role.Name, Description = role.Description, PermissionIds = await PermissionIds(sp, permissions)
            });
            Assert.True(result.Success, result.Error);
        });

    private static async Task<IList<int>> PermissionIds(IServiceProvider sp, string[] names) =>
        (await sp.GetRequiredService<IPermissionService>().GetAllAsync())
            .Where(p => names.Contains(p.Name)).Select(p => p.Id).ToList();
}
