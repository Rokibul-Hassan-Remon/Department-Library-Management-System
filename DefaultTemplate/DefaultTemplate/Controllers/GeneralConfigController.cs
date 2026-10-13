using Default.core.Constants;
using Default.core.DTOs;
using Default.core.Interfaces.Services;
using DefaultTemplate.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DefaultTemplate.Controllers;

public class GeneralConfigController : Controller
{
    private readonly IGeneralConfigService _service;
    public GeneralConfigController(IGeneralConfigService service) => _service = service;

    [HasPermission(Permissions.GeneralConfig.View)]
    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    [HttpGet, HasPermission(Permissions.GeneralConfig.Create)]
    public IActionResult Create() => View(new CreateGeneralConfigDto());

    [HttpPost, HasPermission(Permissions.GeneralConfig.Create), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGeneralConfigDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _service.CreateAsync(dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create.");
            return View(dto);
        }
        TempData["Success"] = "Config created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.GeneralConfig.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return View(new UpdateGeneralConfigDto
        {
            Id = item.Id,
            Key = item.Key,
            Value = item.Value,
            Description = item.Description,
            Group = item.Group
        });
    }

    [HttpPost, HasPermission(Permissions.GeneralConfig.Edit), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateGeneralConfigDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _service.UpdateAsync(dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update.");
            return View(dto);
        }
        TempData["Success"] = "Config updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.GeneralConfig.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete"), HasPermission(Permissions.GeneralConfig.Delete), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success) TempData["Error"] = result.Error;
        else TempData["Success"] = "Config deleted.";
        return RedirectToAction(nameof(Index));
    }
}
