using System.Security.Claims;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Web.Infrastructure;
using CseLibrary.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserService _userService;
    private readonly ISecurityAuditService _securityAudit;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
        IUserService userService, ISecurityAuditService securityAudit, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userService = userService;
        _securityAudit = securityAudit;
        _logger = logger;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // D-24: log only, never the typed value (people type passwords into the email field).
            _logger.LogWarning("Login attempt for an unknown account");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        await _securityAudit.RecordSignInEventAsync(user.Id,
            result.Succeeded ? "LoginSucceeded" : result.IsLockedOut ? "LockedOut" : "LoginFailed");
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) is { } userId)
            await _securityAudit.RecordSignInEventAsync(userId, "Logout");
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult AccessDenied() => View();

    /// <summary>The signed-in user's own profile: details and photo.</summary>
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) is not { } userId) return RedirectToAction("Login");
        var user = await _userService.GetByIdAsync(userId);
        return user == null ? NotFound() : View(user);
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(Pictures.RequestLimit)]
    public async Task<IActionResult> Profile(IFormFile? photo, bool removePhoto)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) is not { } userId) return RedirectToAction("Login");
        var upload = photo.ToImageUpload();
        if (upload == null && !removePhoto) return RedirectToAction(nameof(Profile));

        var result = await _userService.SetPhotoAsync(userId, upload);
        if (!result.Succeeded) TempData["Error"] = result.Error;
        else
        {
            // A new cookie carries the new photo claim, so the top bar shows it at once.
            if (await _userManager.FindByIdAsync(userId) is { } user) await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = upload != null ? "Photo updated." : "Photo removed.";
        }
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) is not { } userId) return RedirectToAction("Login");

        var result = await _userService.ChangeOwnPasswordAsync(userId, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to change password.");
            return View(model);
        }
        // The change gave the user a new security stamp; re-issue the cookie so this session stays signed in.
        if (await _userManager.FindByIdAsync(userId) is { } user) await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction("Index", "Home");
    }
}
