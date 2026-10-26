using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Web.Authorization;
using CseLibrary.Web.Infrastructure;
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

    /// <summary>The author dialog (authors list, book form): JSON { id, name } or 400 { error }.
    /// <paramref name="notify"/>: the list reloads after the dialog, so the toast is queued for it.</summary>
    [HttpPost, HasPermission(Permissions.Books.Create), ValidateAntiForgeryToken, RequestSizeLimit(Pictures.RequestLimit)]
    public async Task<IActionResult> QuickCreate(AuthorFormDto dto, IFormFile? photo, bool notify = false)
    {
        if (!ModelState.IsValid) return BadRequest(new { error = "Enter the author's name (at most 150 characters)." });
        var result = await service.CreateAsync(dto);
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        // The author is saved first; a refused photo (the browser already checked it) only adds a warning.
        if (photo.ToImageUpload() is { } image && await service.SetPhotoAsync(result.Value, image) is { Succeeded: false } refused)
            TempData["Warning"] = $"Author saved, but the photo was not: {refused.Error}";
        var name = (await service.GetAsync(result.Value))!.Name;
        if (notify) TempData["Success"] = $"Author \"{name}\" registered.";
        return Json(new { id = result.Value, name });
    }

    [HttpGet, HasPermission(Permissions.Books.Create)]
    public IActionResult Create() => View("Form", new AuthorFormDto());

    [HttpPost, HasPermission(Permissions.Books.Create), ValidateAntiForgeryToken, RequestSizeLimit(Pictures.RequestLimit)]
    public async Task<IActionResult> Create(AuthorFormDto dto, IFormFile? photo, bool removePhoto)
    {
        if (!ModelState.IsValid) return View("Form", dto);
        var result = await service.CreateAsync(dto);
        if (!result.Succeeded) return FormError(result.Error, dto);
        await this.SavePictureAsync(photo, removePhoto, image => service.SetPhotoAsync(result.Value, image));
        TempData["Success"] = $"Author \"{dto.Name.Trim()}\" registered.";
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet, HasPermission(Permissions.Books.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var author = await service.GetAsync(id);
        return author == null ? NotFound() : View("Form", author);
    }

    [HttpPost, HasPermission(Permissions.Books.Edit), ValidateAntiForgeryToken, RequestSizeLimit(Pictures.RequestLimit)]
    public async Task<IActionResult> Edit(AuthorFormDto dto, IFormFile? photo, bool removePhoto)
    {
        if (!ModelState.IsValid) return View("Form", dto);
        var result = await service.UpdateAsync(dto);
        if (!result.Succeeded) return FormError(result.Error, dto);
        await this.SavePictureAsync(photo, removePhoto, image => service.SetPhotoAsync(dto.Id, image));
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
