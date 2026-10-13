using Default.core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace DefaultTemplate.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = $"{Permissions.ClaimType}:{permission}";
    }
}
