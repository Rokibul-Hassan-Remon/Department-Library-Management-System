using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;
using CseLibrary.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers;

[Authorize]
public class BooksController(IBookService service) : Controller
{
    [HasPermission(Permissions.Books.View)]
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Search"] = search;
        return View(await service.GetPageAsync(new PageRequest(page), search));
    }

    [HasPermission(Permissions.Books.View)]
    public async Task<IActionResult> Details(int id)
    {
        var book = await service.GetDetailsAsync(id);
        return book == null ? NotFound() : View(book);
    }

    [HttpGet, HasPermission(Permissions.Books.Create)]
    public IActionResult Create() => View("Form", new BookFormDto());

    [HttpPost, HasPermission(Permissions.Books.Create), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookFormDto dto)
    {
        if (!ModelState.IsValid) return View("Form", dto);
        var result = await service.CreateAsync(dto);
        if (!result.Succeeded) return FormError(result, dto);
        TempData["Success"] = "Book created. Add its copies below.";
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet, HasPermission(Permissions.Books.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var book = await service.GetDetailsAsync(id);
        return book == null ? NotFound() : View("Form", book);
    }

    [HttpPost, HasPermission(Permissions.Books.Edit), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BookFormDto dto)
    {
        if (!ModelState.IsValid) return View("Form", dto);
        var result = await service.UpdateAsync(dto);
        if (!result.Succeeded) return FormError(result, dto);
        TempData["Success"] = "Book updated.";
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
        TempData["Success"] = "Book deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, HasPermission(Permissions.Books.Edit), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCopy(BookCopyDto dto)
    {
        var result = ModelState.IsValid ? await service.AddCopyAsync(dto) : null;
        if (result is { Succeeded: true }) TempData["Success"] = $"Copy {dto.AccessionNumber} added.";
        else TempData["Error"] = result?.Error ?? "Accession number is required (max 50 characters).";
        return RedirectToAction(nameof(Details), new { id = dto.BookId });
    }

    [HttpGet, HasPermission(Permissions.Books.Edit)]
    public async Task<IActionResult> EditCopy(int id)
    {
        var copy = await service.GetCopyAsync(id);
        return copy == null ? NotFound() : View(copy);
    }

    [HttpPost, HasPermission(Permissions.Books.Edit), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCopy(BookCopyDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await service.UpdateCopyAsync(dto);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update.");
            return View(dto);
        }
        TempData["Success"] = "Copy updated.";
        return RedirectToAction(nameof(Details), new { id = dto.BookId });
    }

    [HttpPost, HasPermission(Permissions.Books.Delete), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCopy(int id, int bookId)
    {
        var result = await service.DeleteCopyAsync(id);
        if (!result.Succeeded) TempData["Error"] = result.Error;
        else TempData["Success"] = "Copy deleted.";
        return RedirectToAction(nameof(Details), new { id = bookId });
    }

    private ViewResult FormError(Result result, BookFormDto dto)
    {
        ModelState.AddModelError(string.Empty, result.Error ?? "Failed to save.");
        return View("Form", dto);
    }
}
