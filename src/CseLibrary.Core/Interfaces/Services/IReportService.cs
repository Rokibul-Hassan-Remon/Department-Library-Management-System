using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Paging;

namespace CseLibrary.Core.Interfaces.Services;

/// <summary>FR-12 reports. Read-only; every list is paged on the server.</summary>
public interface IReportService
{
    /// <summary>Inventory, available, lost and damaged copies (FR-12 items 1, 2, 9).</summary>
    Task<PagedResult<BookCopyDto>> GetCopiesAsync(PageRequest page, CopyStatus? status, CancellationToken ct = default);
    Task<IReadOnlyDictionary<CopyStatus, int>> CountCopiesAsync(CancellationToken ct = default);

    /// <summary>Issued, overdue, member history and the date-range report (items 3–6). Dates are local, both inclusive.</summary>
    Task<PagedResult<LoanDto>> GetLoansAsync(PageRequest page, LoanFilter filter, string? search, DateOnly? from, DateOnly? to, CancellationToken ct = default);

    /// <summary>Student and Fellow membership (items 7, 8).</summary>
    Task<PagedResult<LibraryMemberDto>> GetMembersAsync(PageRequest page, MemberType? type, CancellationToken ct = default);

    /// <summary>Email notification status (item 10).</summary>
    Task<PagedResult<EmailLogDto>> GetEmailsAsync(PageRequest page, DeliveryStatus? status, CancellationToken ct = default);
}
