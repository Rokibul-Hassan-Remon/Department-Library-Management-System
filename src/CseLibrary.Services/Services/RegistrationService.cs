using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services.Services;

/// <summary>
/// D-54: the form never carries a role; registration grants no role or permission (D-37), the portal needs only
/// sign-in. A Pending or Rejected applicant's account is Inactive, so sign-in refuses it (D-32).
/// </summary>
public class RegistrationService(UserManager<ApplicationUser> userManager, IUnitOfWork uow) : IRegistrationService
{
    public Task<Result> RegisterAsync(RegisterDto dto, CancellationToken ct = default) =>
        Enum.IsDefined(dto.RegistrationType) // model binding accepts undefined numbers (D-54 Session 21 item 4)
            ? dto.RegistrationType == RegistrationType.Student ? RegisterStudentAsync(dto, ct) : RegisterFellowAsync(dto, ct)
            : Task.FromResult(Result.Failure(ErrorCodes.InvalidState, "Choose Student or Dept. Fellow."));

    private Task<Result> RegisterStudentAsync(RegisterDto dto, CancellationToken ct) =>
        CreateAsync(dto, MemberType.Student, ApprovalStatus.Approved, ct);

    private Task<Result> RegisterFellowAsync(RegisterDto dto, CancellationToken ct) =>
        CreateAsync(dto, MemberType.Fellow, ApprovalStatus.Pending, ct);

    /// <summary>Shared part: one transaction creates the login and creates or links the member (Q-S19-5).</summary>
    private Task<Result> CreateAsync(RegisterDto dto, MemberType type, ApprovalStatus approval, CancellationToken ct)
    {
        string? userId = null;
        return uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var code = dto.MemberCode.Trim();
            var existingId = await uow.Members.FindIdByCodeAsync(code, attemptCt);
            var member = existingId == null ? null : await uow.Members.GetForUpdateAsync(existingId.Value, attemptCt);
            if (member?.UserId != null)
                return Result.Failure(ErrorCodes.AlreadyRegistered, "This ID is already registered. Sign in instead.");

            var active = approval == ApprovalStatus.Approved;
            var user = new ApplicationUser
            {
                UserName = dto.Email.Trim(), Email = dto.Email.Trim(), FullName = dto.FullName.Trim(), EmailConfirmed = true,
                Status = active ? RecordStatus.Active : RecordStatus.Inactive
            };
            userId = user.Id;
            var create = (await userManager.CreateAsync(user, dto.Password)).ToResult();
            if (!create.Succeeded) return create;

            if (member == null)
            {
                member = new Member { MemberCode = code };
                await uow.Members.AddAsync(member, attemptCt);
            }
            member.FullName = dto.FullName.Trim();
            member.Email = dto.Email.Trim();
            member.MemberType = type;
            member.UserId = user.Id;
            member.Approval = approval;
            member.Status = active ? RecordStatus.Active : RecordStatus.Inactive;
            await uow.AuditLogs.AddAsync(SecurityEvent.For(SecurityEvent.User, user.Id, "UserRegistered", new { Type = type.ToString(), Approval = approval.ToString() }), attemptCt);
            return Result.Success();
        }, verifySucceeded: async c => await uow.Users.GetDetailsAsync(userId!, c) != null, ct: ct)
        .OnDuplicate(Result.Failure(ErrorCodes.Duplicate, "This email or ID is already registered."));
    }

    public Task<IReadOnlyList<PendingRegistrationDto>> GetPendingAsync(CancellationToken ct = default) => uow.Members.GetPendingAsync(ct);

    public Task<Result> ApproveAsync(int memberId, CancellationToken ct = default) => ReviewAsync(memberId, ApprovalStatus.Approved, ct);

    public Task<Result> RejectAsync(int memberId, CancellationToken ct = default) => ReviewAsync(memberId, ApprovalStatus.Rejected, ct);

    private Task<Result> ReviewAsync(int memberId, ApprovalStatus decision, CancellationToken ct) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var member = await uow.Members.GetForUpdateAsync(memberId, attemptCt);
            if (member is not { Approval: ApprovalStatus.Pending, UserId: not null })
                return Result.Failure(ErrorCodes.NotFound, "Pending registration not found.");
            var user = await userManager.FindByIdAsync(member.UserId);
            if (user == null || user.Status == RecordStatus.Deleted) return Result.Failure(ErrorCodes.NotFound, "The applicant's account no longer exists.");

            member.Approval = decision;
            if (decision == ApprovalStatus.Approved)
            {
                member.Status = RecordStatus.Active;
                user.Status = RecordStatus.Active;
                if ((await userManager.UpdateAsync(user)).ToResult() is { Succeeded: false } failed) return failed;
            }
            await uow.AuditLogs.AddAsync(SecurityEvent.For(SecurityEvent.User, user.Id, $"Registration{decision}", null), attemptCt);
            return Result.Success();
        }, ct: ct);
}
