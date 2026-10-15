using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers;

/// <summary>Member self-service (registration, own loans) and the Dept. Fellow review queue (D-54).</summary>
[Authorize]
public class PortalController(IRegistrationService registrations, IMemberService members, ICurrentUser currentUser) : Controller
{
    [HttpGet, AllowAnonymous]
    public IActionResult Register() => View(new RegisterDto());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await registrations.RegisterAsync(dto);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(dto);
        }
        TempData["Success"] = dto.RegistrationType == Core.Entities.RegistrationType.Student
            ? "Registration complete. You can sign in now."
            : "Registration received. You can sign in after the library approves it.";
        return RedirectToAction("Login", "Account");
    }

    /// <summary>The signed-in member's own loans and due dates. Needs no permission, only a linked member.</summary>
    [Authorize]
    public async Task<IActionResult> MyLoans() =>
        View(currentUser.UserId is { } id ? await members.GetForUserAsync(id) : null);

    [HasPermission(Permissions.Registrations.Review)]
    public async Task<IActionResult> Pending() => View(await registrations.GetPendingAsync());

    [HttpPost, HasPermission(Permissions.Registrations.Review), ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int memberId, bool approve)
    {
        var result = approve ? await registrations.ApproveAsync(memberId) : await registrations.RejectAsync(memberId);
        if (result.Succeeded) TempData["Success"] = approve ? "Registration approved." : "Registration rejected.";
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Pending));
    }
}
