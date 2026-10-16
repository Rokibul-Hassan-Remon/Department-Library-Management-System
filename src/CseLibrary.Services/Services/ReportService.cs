using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;

namespace CseLibrary.Services.Services;

public class ReportService(IUnitOfWork uow, TimeProvider time) : IReportService
{
    public Task<PagedResult<BookCopyDto>> GetCopiesAsync(PageRequest page, CopyStatus? status, CancellationToken ct = default) =>
        uow.BookCopies.GetPageAsync(page, status, ct);

    public Task<IReadOnlyDictionary<CopyStatus, int>> CountCopiesAsync(CancellationToken ct = default) =>
        uow.BookCopies.CountByStatusAsync(ct);

    public Task<PagedResult<LoanDto>> GetLoansAsync(PageRequest page, LoanFilter filter, string? search, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);
        // Local calendar days -> UTC instants; "to" is inclusive, so the range ends at the start of the next day.
        var ranged = from != null || to != null;
        // The calendar edges stay open, so 0001-01-01 or 9999-12-31 in the query string cannot overflow.
        var fromUtc = from is { } f && f > DateOnly.MinValue ? ToUtc(f) : DateTime.MinValue;
        var toUtc = to is { } t && t < DateOnly.MaxValue.AddDays(-1) ? ToUtc(t.AddDays(1)) : DateTime.MaxValue;
        return uow.Loans.GetPageAsync(page, filter, search, today, ranged ? fromUtc : null, ranged ? toUtc : null, ct);
    }

    public Task<PagedResult<LibraryMemberDto>> GetMembersAsync(PageRequest page, MemberType? type, CancellationToken ct = default) =>
        uow.LibraryMembers.GetPageAsync(page, null, type, ct);

    public Task<PagedResult<EmailLogDto>> GetEmailsAsync(PageRequest page, DeliveryStatus? status, CancellationToken ct = default) =>
        uow.EmailNotificationLogs.GetPageAsync(page, status, ct);

    private DateTime ToUtc(DateOnly day) => TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), time.LocalTimeZone);
}
