using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers;

/// <summary>FR-12 reports: filters in the query string, printable with the browser's print.</summary>
[Authorize]
public class ReportsController(IReportService service) : Controller
{
    [HasPermission(Permissions.Reports.View)]
    public async Task<IActionResult> Index() => View(await service.CountCopiesAsync());

    [HasPermission(Permissions.Reports.View)]
    public async Task<IActionResult> Copies(int page = 1, CopyStatus? status = null) =>
        View(await service.GetCopiesAsync(new PageRequest(page), status));

    [HasPermission(Permissions.Reports.View)]
    public async Task<IActionResult> Loans(int page = 1, LoanFilter filter = LoanFilter.All, string? search = null, DateOnly? from = null, DateOnly? to = null) =>
        View(await service.GetLoansAsync(new PageRequest(page), filter, search, from, to));

    [HasPermission(Permissions.Reports.View)]
    public async Task<IActionResult> Members(int page = 1, MemberType? type = null) =>
        View(await service.GetMembersAsync(new PageRequest(page), type));

    [HasPermission(Permissions.Reports.View)]
    public async Task<IActionResult> Emails(int page = 1, DeliveryStatus? status = null) =>
        View(await service.GetEmailsAsync(new PageRequest(page), status));
}
