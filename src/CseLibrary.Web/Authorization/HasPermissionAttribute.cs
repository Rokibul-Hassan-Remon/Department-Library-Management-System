using CseLibrary.Core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace CseLibrary.Web.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = $"{Permissions.ClaimType}:{permission}";
    }
}
