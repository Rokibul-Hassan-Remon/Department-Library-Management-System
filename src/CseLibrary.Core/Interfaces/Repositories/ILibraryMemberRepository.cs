using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Paging;

namespace CseLibrary.Core.Interfaces.Repositories;

public interface ILibraryMemberRepository : IRepository<LibraryMember>
{
    /// <summary>Matches member code, name or email; optionally one member type.</summary>
    Task<PagedResult<LibraryMemberDto>> GetPageAsync(PageRequest page, string? search, MemberType? type, CancellationToken ct = default);
    Task<LibraryMemberDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<int?> FindIdByCodeAsync(string memberCode, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string memberCode, int? excludeId, CancellationToken ct = default);
    Task<int?> FindIdByUserIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<PendingRegistrationDto>> GetPendingAsync(CancellationToken ct = default);
}

public interface ILoanRepository : IRepository<Loan>
{
    /// <summary>
    /// Matches member code or name, book title or accession number. With a range, only loans issued or returned in
    /// [<paramref name="fromUtc"/>, <paramref name="toUtc"/>).
    /// </summary>
    Task<PagedResult<LoanDto>> GetPageAsync(PageRequest page, LoanFilter filter, string? search, DateOnly today,
        DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default);
    Task<int> CountActiveAsync(int memberId, CancellationToken ct = default);
    Task<bool> HasOverdueAsync(int memberId, DateOnly today, CancellationToken ct = default);
    /// <summary>Any loan, returned or not: such a member or copy is history and is not deleted (review #6).</summary>
    Task<bool> AnyForMemberAsync(int memberId, CancellationToken ct = default);
    Task<bool> AnyForCopyAsync(int copyId, CancellationToken ct = default);
    /// <summary>Tracked, with its copy and member, for return and renew.</summary>
    Task<Loan?> GetActiveForUpdateAsync(int id, CancellationToken ct = default);
    /// <summary>
    /// Active loans due on or before <paramref name="dueOn"/> (overdue ones included), for members with an email. A range,
    /// not one day, so a day without a run skips no reminder; the idempotency key stops repeats.
    /// </summary>
    Task<IReadOnlyList<ReminderTarget>> GetReminderTargetsAsync(DateOnly dueOn, CancellationToken ct = default);
}
