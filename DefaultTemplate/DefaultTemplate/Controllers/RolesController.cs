using Default.core.Constants;
using Default.core.DTOs;
using Default.core.Interfaces.Services;
using DefaultTemplate.Authorization;
using DefaultTemplate.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DefaultTemplate.Controllers;

public class RolesController : Controller
{
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;

    public RolesController(IRoleService roleService, IPermissionService permissionService)
    {
        _roleService = roleService;
        _permissionService = permissionService;
    }

    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> Index() => View(await _roleService.GetAllAsync());

    [HttpGet, HasPermission(Permissions.Roles.Create)]
    public async Task<IActionResult> Create()
    {
        return View(new CreateRoleViewModel
        {
            AvailablePermissions = (await _permissionService.GetAllAsync()).ToList()
        });
    }

    [HttpPost, HasPermission(Permissions.Roles.Create), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailablePermissions = (await _permissionService.GetAllAsync()).ToList();
            return View(model);
        }

        var dto = new CreateRoleDto
        {
            Name = model.Name,
            Description = model.Description,
            PermissionIds = model.SelectedPermissionIds
        };
        var result = await _roleService.CreateAsync(dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create role.");
            model.AvailablePermissions = (await _permissionService.GetAllAsync()).ToList();
            return View(model);
        }
        TempData["Success"] = "Role created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.Roles.Edit)]
    public async Task<IActionResult> Edit(string id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null) return NotFound();
        return View(new EditRoleViewModel
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            SelectedPermissionIds = role.PermissionIds,
            AvailablePermissions = (await _permissionService.GetAllAsync()).ToList()
        });
    }

    [HttpPost, HasPermission(Permissions.Roles.Edit), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailablePermissions = (await _permissionService.GetAllAsync()).ToList();
            return View(model);
        }

        var dto = new UpdateRoleDto
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            PermissionIds = model.SelectedPermissionIds
        };
        var result = await _roleService.UpdateAsync(dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update role.");
            model.AvailablePermissions = (await _permissionService.GetAllAsync()).ToList();
            return View(model);
        }
        TempData["Success"] = "Role updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.Roles.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null) return NotFound();
        return View(role);
    }

    [HttpPost, ActionName("Delete"), HasPermission(Permissions.Roles.Delete), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var result = await _roleService.DeleteAsync(id);
        if (!result.Success) TempData["Error"] = result.Error;
        else TempData["Success"] = "Role deleted.";
        return RedirectToAction(nameof(Index));
    }
}
