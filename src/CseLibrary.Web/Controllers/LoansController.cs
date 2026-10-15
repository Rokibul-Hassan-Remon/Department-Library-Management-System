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
public class LoansController(ILendingService service) : Controller
{
    [HasPermission(Permissions.Lending.View)]
    public async Task<IActionResult> Index(int page = 1, LoanFilter filter = LoanFilter.Active, string? search = null)
    {
        ViewData["Search"] = search;
        ViewData["Filter"] = filter;
        return View(await service.GetPageAsync(new PageRequest(page), filter, search));
    }

    [HttpPost, HasPermission(Permissions.Lending.Issue), ValidateAntiForgeryToken]
    public async Task<IActionResult> Issue(IssueDto dto, string? returnUrl)
    {
        var result = ModelState.IsValid ? await service.IssueAsync(dto) : Result.Failure(ErrorCodes.NotFound, "Member code and accession number are required.");
        Toast(result, $"Copy {dto.AccessionNumber} issued to {dto.MemberCode}.");
        return Back(returnUrl);
    }

    [HttpPost, HasPermission(Permissions.Lending.Return), ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id, string? returnUrl)
    {
        Toast(await service.ReturnAsync(id), "Book returned.");
        return Back(returnUrl);
    }

    [HttpPost, HasPermission(Permissions.Lending.Issue), ValidateAntiForgeryToken]
    public async Task<IActionResult> Renew(int id, string? returnUrl)
    {
        Toast(await service.RenewAsync(id), "Loan renewed.");
        return Back(returnUrl);
    }

    private void Toast(Result result, string success)
    {
        if (result.Succeeded) TempData["Success"] = success;
        else TempData["Error"] = result.Error;
    }

    private IActionResult Back(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(Index));
}
