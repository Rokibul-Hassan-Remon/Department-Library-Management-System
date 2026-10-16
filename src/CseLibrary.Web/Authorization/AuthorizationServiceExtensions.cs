using System.Security.Claims;
using CseLibrary.Core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace CseLibrary.Web.Authorization;

public static class AuthorizationServiceExtensions
{
    /// <summary>
    /// For views: hide buttons the user may not use. Same database check as [HasPermission] (D-37); cookie
    /// permission claims are ignored. One query per request (handler cache). The server still refuses on its own.
    /// </summary>
    public static async Task<bool> CanAsync(this IAuthorizationService authorization, ClaimsPrincipal user, string permission) =>
        (await authorization.AuthorizeAsync(user, $"{Permissions.ClaimType}:{permission}")).Succeeded;
}
