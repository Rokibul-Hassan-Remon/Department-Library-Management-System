using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Web.Authorization;
using CseLibrary.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UsersController(IUserService userService, RoleManager<ApplicationRole> roleManager)
    {
        _userService = userService;
        _roleManager = roleManager;
    }

    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> Index(int page = 1) => View(await _userService.GetPageAsync(new PageRequest(page)));

    [HttpGet, HasPermission(Permissions.Users.Create)]
    public IActionResult Create()
    {
        return View(new CreateUserViewModel
        {
            AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList()
        });
    }

    [HttpPost, HasPermission(Permissions.Users.Create), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }

        var dto = new CreateUserDto
        {
            UserName = model.UserName,
            Email = model.Email,
            FullName = model.FullName,
            Password = model.Password,
            Roles = model.SelectedRoles
        };
        var result = await _userService.CreateAsync(dto);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create user.");
            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }
        TempData["Success"] = "User created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.Users.Edit)]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        return View(new EditUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            IsActive = user.IsActive,
            SelectedRoles = user.Roles,
            AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList()
        });
    }

    [HttpPost, HasPermission(Permissions.Users.Edit), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }

        var dto = new UpdateUserDto
        {
            Id = model.Id,
            Email = model.Email,
            FullName = model.FullName,
            IsActive = model.IsActive,
            Roles = model.SelectedRoles
        };
        var result = await _userService.UpdateAsync(dto);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update user.");
            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }
        TempData["Success"] = "User updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.Users.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ActionName("Delete"), HasPermission(Permissions.Users.Delete), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result.Succeeded) TempData["Error"] = result.Error;
        else TempData["Success"] = "User deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.Users.Edit)]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(new ResetPasswordViewModel { Id = user.Id, UserName = user.UserName });
    }

    [HttpPost, HasPermission(Permissions.Users.Edit), ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _userService.ResetPasswordAsync(model.Id, model.NewPassword);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to reset password.");
            return View(model);
        }
        TempData["Success"] = "Password reset.";
        return RedirectToAction(nameof(Index));
    }
}
