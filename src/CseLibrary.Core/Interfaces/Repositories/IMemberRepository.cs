using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Paging;

namespace CseLibrary.Core.Interfaces.Repositories;

public interface IMemberRepository : IRepository<Member>
{
    /// <summary>Matches member code, name or email.</summary>
    Task<PagedResult<MemberDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default);
    Task<MemberDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<int?> FindIdByCodeAsync(string memberCode, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string memberCode, int? excludeId, CancellationToken ct = default);
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
}
