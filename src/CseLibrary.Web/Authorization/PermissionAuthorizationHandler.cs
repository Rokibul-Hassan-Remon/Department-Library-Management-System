using CseLibrary.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace CseLibrary.Web.Authorization;

/// <summary>
/// Checks the user's current permissions in the database on every request (D-37): active user, current
/// role membership, current role permissions. Permission claims in the cookie are ignored, so a stale
/// or legacy claim can never grant access. The result is cached for the current request only.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private const string CacheKey = "CseLibrary.Permissions";

    private readonly IPermissionService _permissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAuthorizationHandler(IPermissionService permissionService, IHttpContextAccessor httpContextAccessor)
    {
        _permissionService = permissionService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true) return;

        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        var http = _httpContextAccessor.HttpContext;
        if (http?.Items[CacheKey] is not IReadOnlyList<string> permissions)
        {
            permissions = await _permissionService.GetPermissionsForUserAsync(userId);
            if (http != null) http.Items[CacheKey] = permissions;
        }

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}
