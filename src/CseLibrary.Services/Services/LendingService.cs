using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Services.Services;

/// <summary>
/// Issue, return and renew (LendingTransactions). Policy comes from LibraryPolicies and is read inside the
/// transaction, so a retried attempt re-reads it (D-14). No fines in v1.
/// </summary>
public class LendingService(IUnitOfWork uow, ICurrentUser currentUser, TimeProvider time) : ILendingService
{
    private DateOnly Today => DateOnly.FromDateTime(time.GetLocalNow().DateTime);

    public Task<PagedResult<LoanDto>> GetPageAsync(PageRequest page, LoanFilter filter, string? search, CancellationToken ct = default) =>
        uow.Loans.GetPageAsync(page, filter, search, Today, ct);

    public Task<Result> IssueAsync(IssueDto dto, CancellationToken ct = default)
    {
        if (currentUser.UserId is not { } staffId) return Task.FromResult(SignInRequired());
        return uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var memberId = await uow.Members.FindIdByCodeAsync(dto.MemberCode.Trim(), attemptCt);
            if (memberId == null) return Result.Failure(ErrorCodes.NotFound, "Member not found.");
            // One member's issues run one at a time, so the limit check and the insert cannot interleave (D-48 P12).
            await uow.AcquireLockAsync($"CseLibrary.Member.{memberId}", attemptCt);

            var member = (await uow.Members.GetForUpdateAsync(memberId.Value, attemptCt))!;
            if (member.Status != RecordStatus.Active) return Result.Failure(ErrorCodes.MemberInactive, "The member is not active.");
            if (await uow.Loans.HasOverdueAsync(member.Id, Today, attemptCt))
                return Result.Failure(ErrorCodes.HasOverdue, "The member has an overdue book; return it first.");
            if (await uow.LibraryPolicies.GetForTypeAsync(member.MemberType, attemptCt) is not { } policy) return NoPolicy(member.MemberType);
            if (await uow.Loans.CountActiveAsync(member.Id, attemptCt) >= policy.MaxActiveLoans)
                return Result.Failure(ErrorCodes.LimitReached, $"The member already has {policy.MaxActiveLoans} books (the limit for a {member.MemberType}).");

            var copy = await uow.BookCopies.FindByAccessionForUpdateAsync(dto.AccessionNumber.Trim(), attemptCt);
            if (copy == null) return Result.Failure(ErrorCodes.NotFound, "Copy not found.");
            if (copy.CopyStatus != CopyStatus.Available)
                return Result.Failure(ErrorCodes.CopyUnavailable, $"This copy is {copy.CopyStatus}, not available.");

            copy.CopyStatus = CopyStatus.Issued; // RowVersion: a concurrent change to this copy fails this save
            await uow.Loans.AddAsync(new Loan
            {
                MemberId = member.Id, BookCopyId = copy.Id, IssueDateUtc = time.GetUtcNow().UtcDateTime,
                DueDate = Today.AddDays(policy.LoanDays), IssuedByUserId = staffId
            }, attemptCt);
            return Result.Success();
        }, ct: ct).OnDuplicate(CopyTaken()).OnConflict(CopyTaken());
    }

    public Task<Result> ReturnAsync(int loanId, ReturnCondition condition = ReturnCondition.Good, CancellationToken ct = default)
    {
        if (currentUser.UserId is not { } staffId) return Task.FromResult(SignInRequired());
        if (!Enum.IsDefined(condition)) return Task.FromResult(Result.Failure(ErrorCodes.InvalidState, "Unknown return condition."));
        return uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var loan = await uow.Loans.GetActiveForUpdateAsync(loanId, attemptCt);
            if (loan == null) return Result.Failure(ErrorCodes.NotFound, "Active loan not found.");
            loan.TransactionStatus = LendingStatus.Returned;
            loan.ReturnDateUtc = time.GetUtcNow().UtcDateTime;
            loan.ReturnedByUserId = staffId;
            loan.ReturnCondition = condition;
            loan.BookCopy.CopyStatus = condition == ReturnCondition.Damaged ? CopyStatus.Damaged : CopyStatus.Available;
            return Result.Success();
        }, ct: ct).OnConflict(Result.Failure(ErrorCodes.InvalidState, "This loan or copy was just changed. Reload and try again."));
    }

    public Task<Result> RenewAsync(int loanId, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var loan = await uow.Loans.GetActiveForUpdateAsync(loanId, attemptCt);
            if (loan == null) return Result.Failure(ErrorCodes.NotFound, "Active loan not found.");
            if (loan.DueDate < Today) return Result.Failure(ErrorCodes.RenewNotAllowed, "An overdue book must be returned, not renewed.");
            if (await uow.LibraryPolicies.GetForTypeAsync(loan.Member.MemberType, attemptCt) is not { } policy) return NoPolicy(loan.Member.MemberType);
            if (loan.RenewCount >= policy.MaxRenewals)
                return Result.Failure(ErrorCodes.RenewNotAllowed, $"This loan was already renewed {loan.RenewCount} time(s); the limit is {policy.MaxRenewals}.");

            loan.RenewCount++;
            loan.DueDate = Today.AddDays(policy.LoanDays);
            return Result.Success();
        }, ct: ct);

    public Task<IReadOnlyList<LibraryPolicyDto>> GetPoliciesAsync(CancellationToken ct = default) => uow.LibraryPolicies.GetListAsync(ct);

    public async Task<Result> UpdatePolicyAsync(LibraryPolicyDto dto, CancellationToken ct = default)
    {
        var policy = await uow.LibraryPolicies.GetForUpdateAsync(dto.Id, ct);
        if (policy == null) return Result.Failure(ErrorCodes.NotFound, "Policy not found.");
        policy.MaxActiveLoans = dto.MaxActiveLoans;
        policy.LoanDays = dto.LoanDays;
        policy.MaxRenewals = dto.MaxRenewals;
        return await uow.SaveAsync(ct);
    }

    private static Result CopyTaken() => Result.Failure(ErrorCodes.CopyUnavailable, "This copy was just issued to someone else.");
    private static Result SignInRequired() => Result.Failure(ErrorCodes.Forbidden, "Sign in as library staff to issue or return books.");
    private static Result NoPolicy(MemberType type) => Result.Failure(ErrorCodes.InvalidState, $"No lending policy is set for {type} members.");
}
