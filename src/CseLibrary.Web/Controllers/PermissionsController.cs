using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Web.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers;

public class PermissionsController : Controller
{
    private readonly IPermissionService _service;
    public PermissionsController(IPermissionService service) => _service = service;

    [HasPermission(Permissions.PermissionsModule.View)]
    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    [HttpGet, HasPermission(Permissions.PermissionsModule.Create)]
    public IActionResult Create() => View(new CreatePermissionDto());

    [HttpPost, HasPermission(Permissions.PermissionsModule.Create), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePermissionDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _service.CreateAsync(dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create.");
            return View(dto);
        }
        TempData["Success"] = "Permission created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.PermissionsModule.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return View(new UpdatePermissionDto
        {
            Id = item.Id,
            Name = item.Name,
            DisplayName = item.DisplayName,
            Module = item.Module,
            Description = item.Description
        });
    }

    [HttpPost, HasPermission(Permissions.PermissionsModule.Edit), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdatePermissionDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _service.UpdateAsync(dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update.");
            return View(dto);
        }
        TempData["Success"] = "Permission updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.PermissionsModule.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete"), HasPermission(Permissions.PermissionsModule.Delete), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success) TempData["Error"] = result.Error;
        else TempData["Success"] = "Permission deleted.";
        return RedirectToAction(nameof(Index));
    }
}
