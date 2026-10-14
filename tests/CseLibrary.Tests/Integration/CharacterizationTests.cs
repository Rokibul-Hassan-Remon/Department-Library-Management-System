using System.Net;
using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CseLibrary.Tests.Integration;

/// <summary>
/// Characterization (step 0.5, D-43): pins the template's behaviour as it is today, defects included,
/// so Gate 2 refactoring shows exactly what changed. Tests marked "defect" are expected to flip later.
/// </summary>
[Collection(AppCollection.Name)]
public class CharacterizationTests(AppFactory app)
{
    private const string UserPassword = "Passw0rd!12";

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

    [Fact] // A9 (D-37, fixed in 0.9; was a defect): login writes nothing to AspNetUserClaims
    public async Task Login_writes_no_permission_claims_to_the_database()
    {
        var (email, _) = await CreateUserWithPermissions(Permissions.GeneralConfig.View);
        await app.LoggedInClient(email, UserPassword);

        var claims = await app.WithServiceAsync(async sp =>
        {
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            return await users.GetClaimsAsync((await users.FindByEmailAsync(email))!);
        });
        Assert.Empty(claims);
    }

    [Fact]
    public async Task Anonymous_request_to_a_protected_page_redirects_to_login()
    {
        var response = await app.NewClient().GetAsync("/GeneralConfig");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Theory] // A1 (D-31): the public allowlist answers anonymously; everything else needs a login
    [InlineData("/", HttpStatusCode.OK)]
    [InlineData("/Home/Privacy", HttpStatusCode.OK)]
    [InlineData("/Home/Error", HttpStatusCode.OK)]
    [InlineData("/Account/Login", HttpStatusCode.OK)]
    [InlineData("/Account/ChangePassword", HttpStatusCode.Redirect)]
    [InlineData("/Users", HttpStatusCode.Redirect)]
    public async Task Only_the_public_allowlist_is_anonymous(string url, HttpStatusCode expected) =>
        Assert.Equal(expected, (await app.NewClient().GetAsync(url)).StatusCode);

    [Theory] // A2 (D-55, Q-S19-10): back to a safe local page with a red toast; never AccessDenied, never off-site
    [InlineData("https://localhost/Account/ChangePassword?x=1", "/Account/ChangePassword?x=1")]
    [InlineData(null, "/")]                                       // typed URL: Home
    [InlineData("https://evil.example/phish", "/")]               // other site
    [InlineData("https://localhost//evil.example/phish", "/")]    // protocol-relative path
    [InlineData("https://localhost/GeneralConfig", "/")]          // the denied page itself: no loop
    public async Task Permission_failure_returns_to_a_safe_page_with_a_toast(string? referer, string expected)
    {
        var (email, _) = await CreateUserWithPermissions();
        var client = await app.LoggedInClient(email, UserPassword);
        var request = new HttpRequestMessage(HttpMethod.Get, "/GeneralConfig");
        if (referer != null) request.Headers.Referrer = new Uri(referer);

        var denied = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
        Assert.Equal(expected, denied.Headers.Location?.OriginalString);
        Assert.Contains("You don&#x27;t have permission to perform this action.", await client.GetStringAsync(expected));
    }

    [Fact] // A2 (D-55): AJAX requests get a plain 403, no redirect
    public async Task Ajax_permission_failure_is_403()
    {
        var (email, _) = await CreateUserWithPermissions();
        var client = await app.LoggedInClient(email, UserPassword);
        var request = new HttpRequestMessage(HttpMethod.Get, "/GeneralConfig");
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(request)).StatusCode);
    }

    [Fact] // D-55: the existing TempData messages render as one toast (label text, close button)
    public async Task Success_message_renders_as_a_toast()
    {
        var client = await app.LoggedInClient(AppFactory.AdminEmail, AppFactory.AdminPassword);
        var key = "Toast." + Guid.NewGuid().ToString("N")[..8];
        var created = await AppFactory.PostFormAsync(client, "/GeneralConfig/Create", "/GeneralConfig/Create", new()
        {
            ["Key"] = key, ["Value"] = "v", ["Group"] = "Test", ["Description"] = ""
        });
        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);

        var html = await client.GetStringAsync(created.Headers.Location!.OriginalString);
        Assert.Contains("class=\"toast ", html);
        Assert.Contains("<strong>Success:</strong>", html);
        Assert.Contains("data-bs-dismiss=\"toast\"", html);
    }

    [Fact] // A11 (D-31): an endpoint with no authorization metadata is still guarded by the fallback policy
    public async Task Endpoint_without_authorization_metadata_requires_login()
    {
        var response = await app.NewClient().GetAsync("/FallbackProbe");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
        var signedIn = await app.LoggedInClient(AppFactory.AdminEmail, AppFactory.AdminPassword);
        Assert.Equal("reached", await signedIn.GetStringAsync("/FallbackProbe")); // the probe itself is reachable
    }

    [Fact] // A5 (D-31): a POST without an antiforgery token is refused with 400
    public async Task Post_without_antiforgery_token_is_rejected()
    {
        var response = await app.NewClient().PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = AppFactory.AdminEmail, ["Password"] = AppFactory.AdminPassword, ["RememberMe"] = "false"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact] // A3 (D-31): 5 failures lock the account; the right password is then refused too
    public async Task Five_failed_logins_lock_the_account()
    {
        var (email, _) = await CreateUserWithPermissions();
        for (var i = 0; i < 5; i++) await PostLogin(email, "wrong-password-1");

        var response = await PostLogin(email, UserPassword);

        Assert.Contains("Invalid login attempt.", await response.Content.ReadAsStringAsync());
        Assert.True(await app.WithServiceAsync(async sp =>
        {
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            return await users.IsLockedOutAsync((await users.FindByEmailAsync(email))!);
        }));
    }

    [Theory] // D-31: password policy from appsettings — min 10 with a digit, no other character rules
    [InlineData("Abcdef-g1", false)]  // 9 chars: fails on length alone
    [InlineData("abcdefghij", false)] // no digit
    [InlineData("abcdefgh12", true)]  // Identity's defaults would refuse this (no upper case, no symbol)
    public async Task Password_policy_comes_from_configuration(string password, bool accepted)
    {
        var email = $"pw_{Guid.NewGuid():N}@test.local";
        var result = await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().CreateAsync(new CreateUserDto
        {
            UserName = email, Email = email, Password = password
        }));
        Assert.Equal(accepted, result.Succeeded);
    }

    [Fact] // D-31: auth cookie is Secure, HttpOnly, SameSite=Lax; stamp checked every 5 min; antiforgery is global
    public async Task Cookie_stamp_and_antiforgery_settings_are_applied()
    {
        var response = await PostLogin(AppFactory.AdminEmail, AppFactory.AdminPassword);
        var cookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith(".AspNetCore.Identity.Application="));
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);

        var sp = app.Services;
        Assert.Equal(TimeSpan.FromMinutes(5), sp.GetRequiredService<IOptions<SecurityStampValidatorOptions>>().Value.ValidationInterval);
        Assert.Contains(sp.GetRequiredService<IOptions<MvcOptions>>().Value.Filters, f => f is AutoValidateAntiforgeryTokenAttribute);
    }

    private Task<HttpResponseMessage> PostLogin(string email, string password) =>
        AppFactory.PostFormAsync(app.NewClient(), "/Account/Login", "/Account/Login", new()
        {
            ["Email"] = email, ["Password"] = password, ["RememberMe"] = "false"
        });

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

    // A7 (D-37, fixed in 0.9). The old defect: each cookie carried the permissions written by the
    // *previous* login, so a revoked permission came back for exactly one later login.
    [Fact]
    public async Task Revoked_permission_is_denied_on_the_next_request_and_after_the_next_login()
    {
        var (email, roleId) = await CreateUserWithPermissions(Permissions.GeneralConfig.View);
        var first = await app.LoggedInClient(email, UserPassword);
        Assert.Equal(HttpStatusCode.OK, (await first.GetAsync("/GeneralConfig")).StatusCode);

        await SetRolePermissions(roleId);

        Assert.Equal(HttpStatusCode.Redirect, (await first.GetAsync("/GeneralConfig")).StatusCode);
        var second = await app.LoggedInClient(email, UserPassword);
        Assert.Equal(HttpStatusCode.Redirect, (await second.GetAsync("/GeneralConfig")).StatusCode);
    }

    // A7 (D-37): a legacy permission row in AspNetUserClaims goes into the cookie (the default claims
    // factory copies user claims), but the handler ignores cookie permission claims, so it grants nothing.
    [Fact]
    public async Task Legacy_permission_claim_in_the_cookie_grants_nothing()
    {
        var (email, _) = await CreateUserWithPermissions();
        await app.WithServiceAsync(async sp =>
        {
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var claim = new System.Security.Claims.Claim(Permissions.ClaimType, Permissions.GeneralConfig.View);
            Assert.True((await users.AddClaimAsync((await users.FindByEmailAsync(email))!, claim)).Succeeded);
        });

        var client = await app.LoggedInClient(email, UserPassword);
        var response = await client.GetAsync("/GeneralConfig");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString); // D-55: Home with a toast, not AccessDenied
    }

    [Fact] // A8 (D-37): removing the user from the role denies on the next request with the same cookie
    public async Task Removed_from_role_is_denied_on_the_next_request()
    {
        var (email, _) = await CreateUserWithPermissions(Permissions.GeneralConfig.View);
        var client = await app.LoggedInClient(email, UserPassword);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/GeneralConfig")).StatusCode);

        await app.WithServiceAsync(async sp =>
        {
            var user = (await sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email))!;
            var result = await sp.GetRequiredService<IUserService>().UpdateAsync(new UpdateUserDto
            {
                Id = user.Id, Email = email, FullName = user.FullName, IsActive = true, Roles = []
            });
            Assert.True(result.Succeeded, result.Error);
        });

        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/GeneralConfig")).StatusCode);
    }

    [Fact] // D-37: the menu uses the same database check; a legacy claim shows nothing
    public async Task Menu_shows_links_for_database_permissions_only()
    {
        var (email, _) = await CreateUserWithPermissions(Permissions.GeneralConfig.View);
        await app.WithServiceAsync(async sp =>
        {
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var claim = new System.Security.Claims.Claim(Permissions.ClaimType, Permissions.Users.View);
            Assert.True((await users.AddClaimAsync((await users.FindByEmailAsync(email))!, claim)).Succeeded);
        });

        var html = await (await app.LoggedInClient(email, UserPassword)).GetStringAsync("/");

        Assert.Contains("href=\"/GeneralConfig\"", html);
        Assert.DoesNotContain("href=\"/Users\"", html);
    }

    [Fact] // current behaviour: a granted permission works on the next request (DB fallback in the handler)
    public async Task Granted_permission_works_on_the_next_request()
    {
        var (email, roleId) = await CreateUserWithPermissions();
        var client = await app.LoggedInClient(email, UserPassword);
        var denied = await client.GetAsync("/GeneralConfig");
        Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
        Assert.Equal("/", denied.Headers.Location?.OriginalString); // D-55

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
            Assert.True(role.Succeeded, role.Error);
            return role.Value!;
        });

        var email = $"user_{suffix}@test.local";
        var user = await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().CreateAsync(new CreateUserDto
        {
            UserName = email, Email = email, Password = UserPassword, Roles = ["Role_" + suffix]
        }));
        Assert.True(user.Succeeded, user.Error);
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
            Assert.True(result.Succeeded, result.Error);
        });

    private static async Task<IList<int>> PermissionIds(IServiceProvider sp, string[] names) =>
        (await sp.GetRequiredService<IPermissionService>().GetAllAsync())
            .Where(p => names.Contains(p.Name)).Select(p => p.Id).ToList();
}
