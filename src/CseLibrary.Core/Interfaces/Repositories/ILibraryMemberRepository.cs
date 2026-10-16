using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Paging;

namespace CseLibrary.Core.Interfaces.Repositories;

public interface ILibraryMemberRepository : IRepository<LibraryMember>
{
    /// <summary>Matches member code, name or email.</summary>
    Task<PagedResult<LibraryMemberDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default);
    Task<LibraryMemberDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<int?> FindIdByCodeAsync(string memberCode, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string memberCode, int? excludeId, CancellationToken ct = default);
    Task<int?> FindIdByUserIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<PendingRegistrationDto>> GetPendingAsync(CancellationToken ct = default);
}

public interface ILoanRepository : IRepository<Loan>
{
    /// <summary>Matches member code or name, book title or accession number.</summary>
    Task<PagedResult<LoanDto>> GetPageAsync(PageRequest page, LoanFilter filter, string? search, DateOnly today, CancellationToken ct = default);
    Task<int> CountActiveAsync(int memberId, CancellationToken ct = default);
    Task<bool> HasOverdueAsync(int memberId, DateOnly today, CancellationToken ct = default);
    Task<bool> HasActiveAsync(int memberId, CancellationToken ct = default);
    /// <summary>Tracked, with its copy and member, for return and renew.</summary>
    Task<Loan?> GetActiveForUpdateAsync(int id, CancellationToken ct = default);
    /// <summary>Active loans due on <paramref name="dueOn"/> or before <paramref name="today"/>, for members with an email.</summary>
    Task<IReadOnlyList<ReminderTarget>> GetReminderTargetsAsync(DateOnly dueOn, DateOnly today, CancellationToken ct = default);
}
