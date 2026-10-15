using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers;

[Authorize]
public class MembersController(IMemberService service) : Controller
{
    [HasPermission(Permissions.Members.View)]
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Search"] = search;
        return View(await service.GetPageAsync(new PageRequest(page), search));
    }

    [HasPermission(Permissions.Members.View)]
    public async Task<IActionResult> Details(int id)
    {
        var member = await service.GetDetailsAsync(id);
        return member == null ? NotFound() : View(member);
    }

    [HttpGet, HasPermission(Permissions.Members.Create)]
    public IActionResult Create() => View("Form", new MemberDto());

    [HttpPost, HasPermission(Permissions.Members.Create), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MemberDto dto)
    {
        if (!ModelState.IsValid) return View("Form", dto);
        var result = await service.CreateAsync(dto);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View("Form", dto);
        }
        TempData["Success"] = "Member created.";
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet, HasPermission(Permissions.Members.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var member = await service.GetDetailsAsync(id);
        return member == null ? NotFound() : View("Form", member);
    }

    [HttpPost, HasPermission(Permissions.Members.Edit), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MemberDto dto)
    {
        if (!ModelState.IsValid) return View("Form", dto);
        var result = await service.UpdateAsync(dto);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View("Form", dto);
        }
        TempData["Success"] = "Member updated.";
        return RedirectToAction(nameof(Details), new { id = dto.Id });
    }

    [HttpPost, HasPermission(Permissions.Members.Delete), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id });
        }
        TempData["Success"] = "Member deleted.";
        return RedirectToAction(nameof(Index));
    }
}
