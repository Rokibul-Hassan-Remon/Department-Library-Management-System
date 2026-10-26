using System.Security.Claims;
using CseLibrary.Core.Constants;
using CseLibrary.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CseLibrary.Data;

/// <summary>
/// Puts the user's photo path in the login cookie, so the top bar shows it without a query per page.
/// Refreshed on the user's own change (RefreshSignInAsync) and at each security-stamp check (30 min default).
/// </summary>
public class PhotoClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (user.PhotoPath != null) identity.AddClaim(new Claim(AppClaims.Photo, user.PhotoPath));
        return identity;
    }
}
