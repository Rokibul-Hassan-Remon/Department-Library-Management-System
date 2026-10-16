using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Core.Interfaces.Services;

public interface ILibraryMemberService
{
    Task<PagedResult<LibraryMemberDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default);
    Task<LibraryMemberDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    /// <summary>The member linked to a portal login, or null.</summary>
    Task<LibraryMemberDetailsDto?> GetForUserAsync(string userId, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(LibraryMemberDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(LibraryMemberDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}

public interface ILendingService
{
    Task<PagedResult<LoanDto>> GetPageAsync(PageRequest page, LoanFilter filter, string? search, CancellationToken ct = default);
    Task<Result> IssueAsync(IssueDto dto, CancellationToken ct = default);
    Task<Result> ReturnAsync(int loanId, ReturnCondition condition = ReturnCondition.Good, CancellationToken ct = default);
    Task<IReadOnlyList<LibraryPolicyDto>> GetPoliciesAsync(CancellationToken ct = default);
    Task<Result> UpdatePolicyAsync(LibraryPolicyDto dto, CancellationToken ct = default);
    Task<Result> RenewAsync(int loanId, CancellationToken ct = default);
}
