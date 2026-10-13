using System.Security.Claims;
using CseLibrary.Core.Constants;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPermissionService _permissionService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IPermissionService permissionService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _permissionService = permissionService;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        await AddPermissionClaimsAsync(user);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [HttpGet, Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }
        await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction("Index", "Home");
    }

    private async Task AddPermissionClaimsAsync(ApplicationUser user)
    {
        var existing = await _userManager.GetClaimsAsync(user);
        var stale = existing.Where(c => c.Type == Permissions.ClaimType).ToList();
        if (stale.Count > 0) await _userManager.RemoveClaimsAsync(user, stale);

        var perms = await _permissionService.GetPermissionsForUserAsync(user.Id);
        var claims = perms.Select(p => new Claim(Permissions.ClaimType, p)).ToList();
        if (claims.Count > 0)
        {
            await _userManager.AddClaimsAsync(user, claims);
            await _signInManager.RefreshSignInAsync(user);
        }
    }
}
