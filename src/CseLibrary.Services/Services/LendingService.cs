using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Services.Services;

/// <summary>
/// Issue, return and renew. Policy (limits, loan days, renewals) is read from GeneralConfig inside the
/// transaction, so a retried attempt re-reads it (D-14). No fines in v1.
/// </summary>
public class LendingService(IUnitOfWork uow, TimeProvider time) : ILendingService
{
    private DateOnly Today => DateOnly.FromDateTime(time.GetLocalNow().DateTime);

    public Task<PagedResult<LoanDto>> GetPageAsync(PageRequest page, LoanFilter filter, string? search, CancellationToken ct = default) =>
        uow.Loans.GetPageAsync(page, filter, search, Today, ct);

    public Task<Result> IssueAsync(IssueDto dto, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var memberId = await uow.Members.FindIdByCodeAsync(dto.MemberCode.Trim(), attemptCt);
            if (memberId == null) return Result.Failure(ErrorCodes.NotFound, "Member not found.");
            // One member's issues run one at a time, so the limit check and the insert cannot interleave (D-48 P12).
            await uow.AcquireLockAsync($"CseLibrary.Member.{memberId}", attemptCt);

            var member = (await uow.Members.GetForUpdateAsync(memberId.Value, attemptCt))!;
            if (member.Status != RecordStatus.Active) return Result.Failure(ErrorCodes.MemberInactive, "The member is not active.");
            if (await uow.Loans.HasOverdueAsync(member.Id, Today, attemptCt))
                return Result.Failure(ErrorCodes.HasOverdue, "The member has an overdue book; return it first.");
            var max = await PolicyAsync(LendingConfigKeys.MaxLoans(member.MemberType), attemptCt);
            if (await uow.Loans.CountActiveAsync(member.Id, attemptCt) >= max)
                return Result.Failure(ErrorCodes.LimitReached, $"The member already has {max} books (the limit for a {member.MemberType}).");

            var copy = await uow.BookCopies.FindByAccessionForUpdateAsync(dto.AccessionNumber.Trim(), attemptCt);
            if (copy == null) return Result.Failure(ErrorCodes.NotFound, "Copy not found.");
            if (copy.CopyStatus != CopyStatus.Available)
                return Result.Failure(ErrorCodes.CopyUnavailable, $"This copy is {copy.CopyStatus}, not available.");

            var days = await PolicyAsync(LendingConfigKeys.LoanDays(member.MemberType), attemptCt);
            copy.CopyStatus = CopyStatus.Issued;
            await uow.Loans.AddAsync(new Loan
            {
                MemberId = member.Id, BookCopyId = copy.Id, IssuedAtUtc = time.GetUtcNow().UtcDateTime, DueDate = Today.AddDays(days)
            }, attemptCt);
            return Result.Success();
        }, ct: ct).OnDuplicate(Result.Failure(ErrorCodes.CopyUnavailable, "This copy was just issued to someone else."));

    public Task<Result> ReturnAsync(int loanId, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var loan = await uow.Loans.GetActiveForUpdateAsync(loanId, attemptCt);
            if (loan == null) return Result.Failure(ErrorCodes.NotFound, "Active loan not found.");
            loan.ReturnedAtUtc = time.GetUtcNow().UtcDateTime;
            loan.BookCopy.CopyStatus = CopyStatus.Available;
            return Result.Success();
        }, ct: ct);

    public Task<Result> RenewAsync(int loanId, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var loan = await uow.Loans.GetActiveForUpdateAsync(loanId, attemptCt);
            if (loan == null) return Result.Failure(ErrorCodes.NotFound, "Active loan not found.");
            if (loan.DueDate < Today) return Result.Failure(ErrorCodes.RenewNotAllowed, "An overdue book must be returned, not renewed.");
            var maxRenewals = await PolicyAsync(LendingConfigKeys.MaxRenewals, attemptCt);
            if (loan.RenewCount >= maxRenewals)
                return Result.Failure(ErrorCodes.RenewNotAllowed, $"This loan was already renewed {loan.RenewCount} time(s); the limit is {maxRenewals}.");

            loan.RenewCount++;
            loan.DueDate = Today.AddDays(await PolicyAsync(LendingConfigKeys.LoanDays(loan.Member.MemberType), attemptCt));
            return Result.Success();
        }, ct: ct);

    /// <summary>A missing or unreadable value falls back to the built-in default.</summary>
    private async Task<int> PolicyAsync(string key, CancellationToken ct) =>
        int.TryParse((await uow.GeneralConfigs.GetByKeyAsync(key, ct))?.Value, out var v) && v >= 0 ? v : LendingConfigKeys.DefaultFor(key);
}
