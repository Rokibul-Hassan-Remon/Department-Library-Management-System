using Microsoft.AspNetCore.Authorization;

namespace CseLibrary.Web.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}
