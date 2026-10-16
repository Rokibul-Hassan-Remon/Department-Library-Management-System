using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers;

/// <summary>The Authors register; part of the catalog, so it uses the Books permissions.</summary>
[Authorize]
public class AuthorsController(IAuthorService service, IBookService books) : Controller
{
    [HasPermission(Permissions.Books.View)]
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Search"] = search;
        return View(await service.GetPageAsync(new PageRequest(page), search));
    }

    /// <summary>The author and their books, server-paged.</summary>
    [HasPermission(Permissions.Books.View)]
    public async Task<IActionResult> Details(int id, int page = 1)
    {
        var author = await service.GetAsync(id);
        if (author == null) return NotFound();
        ViewData["Books"] = await books.GetPageAsync(new PageRequest(page), null, authorId: id);
        return View(author);
    }

    /// <summary>Select2 source for the book form: { results: [{ id, text }] }.</summary>
    [HasPermission(Permissions.Books.View)]
    public async Task<IActionResult> Lookup(string? term) =>
        Json(new { results = (await service.SearchAsync(term)).Select(a => new { id = a.Id, text = a.Name }) });

    [HttpGet, HasPermission(Permissions.Books.Create)]
    public IActionResult Create() => View("Form", new AuthorFormDto());

    [HttpPost, HasPermission(Permissions.Books.Create), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AuthorFormDto dto)
    {
        if (!ModelState.IsValid) return View("Form", dto);
        var result = await service.CreateAsync(dto);
        if (!result.Succeeded) return FormError(result.Error, dto);
        TempData["Success"] = $"Author \"{dto.Name.Trim()}\" registered.";
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet, HasPermission(Permissions.Books.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var author = await service.GetAsync(id);
        return author == null ? NotFound() : View("Form", author);
    }

    [HttpPost, HasPermission(Permissions.Books.Edit), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AuthorFormDto dto)
    {
        if (!ModelState.IsValid) return View("Form", dto);
        var result = await service.UpdateAsync(dto);
        if (!result.Succeeded) return FormError(result.Error, dto);
        TempData["Success"] = "Author updated.";
        return RedirectToAction(nameof(Details), new { id = dto.Id });
    }

    [HttpPost, HasPermission(Permissions.Books.Delete), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id });
        }
        TempData["Success"] = "Author deleted.";
        return RedirectToAction(nameof(Index));
    }

    private ViewResult FormError(string? error, AuthorFormDto dto)
    {
        ModelState.AddModelError(string.Empty, error ?? "Failed to save.");
        return View("Form", dto);
    }
}
