using Default.core.Constants;
using Default.core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace DefaultTemplate.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
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

        if (user.HasClaim(c => c.Type == Permissions.ClaimType && c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        var permissions = await _permissionService.GetPermissionsForUserAsync(userId);
        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}
