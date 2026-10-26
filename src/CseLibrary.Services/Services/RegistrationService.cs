using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services.Services;

/// <summary>
/// D-54: the form never carries a role. The only role a member gets is the configured non-privileged default for
/// their type (D-56 Q-S21-1): a Student at registration, a Fellow on approval. The portal itself needs only sign-in.
/// A Pending or Rejected applicant's account is Inactive, so sign-in refuses it (D-32).
/// </summary>
public class RegistrationService(UserManager<ApplicationUser> userManager, ILookupNormalizer normalizer, IUnitOfWork uow, IImageStore images) : IRegistrationService
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
        return ImageAttachment.CreateWithAsync(images, ImageKind.Users, dto.Photo, photo => uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var code = dto.MemberCode.Trim();
            var existingId = await uow.LibraryMembers.FindIdByCodeAsync(code, attemptCt);
            var member = existingId == null ? null : await uow.LibraryMembers.GetForUpdateAsync(existingId.Value, attemptCt);
            if (member?.IdentityUserId != null)
                return Result.Failure(ErrorCodes.AlreadyRegistered, "This ID is already registered. Sign in instead.");

            // Review #1: an existing ID is linked at once only when the email matches the staff record. Otherwise a
            // reviewer decides, and the record (name, type, status) stays as staff left it.
            var isNew = member == null;
            var claim = !isNew && !string.Equals(member!.Email, dto.Email.Trim(), StringComparison.OrdinalIgnoreCase);
            var decided = claim ? ApprovalStatus.Pending : approval;
            var active = decided == ApprovalStatus.Approved;
            var user = new ApplicationUser
            {
                UserName = dto.Email.Trim(), Email = dto.Email.Trim(), FullName = dto.FullName.Trim(), EmailConfirmed = true,
                Status = active ? RecordStatus.Active : RecordStatus.Inactive, PhotoPath = photo
            };
            userId = user.Id;
            var create = (await userManager.CreateAsync(user, dto.Password)).ToResult();
            if (!create.Succeeded) return create;
            var role = active ? await AssignDefaultRoleAsync(user, type, attemptCt) : null;

            if (member == null)
            {
                member = new LibraryMember { MemberCode = code };
                await uow.LibraryMembers.AddAsync(member, attemptCt);
            }
            if (!claim)
            {
                member.FullName = dto.FullName.Trim();
                member.Email = dto.Email.Trim();
                member.MemberType = type;
                if (isNew || !active) member.Status = active ? RecordStatus.Active : RecordStatus.Inactive; // keeps a staff deactivation (BR-03)
            }
            member.IdentityUserId = user.Id;
            member.Approval = decided;
            await uow.AuditLogs.AddAsync(SecurityEvent.For(SecurityEvent.User, user.Id, "UserRegistered", new { Type = type.ToString(), Approval = decided.ToString(), Role = role, Claim = claim }), attemptCt);
            return Result.Success();
        }, verifySucceeded: async c => await uow.Users.GetDetailsAsync(userId!, c) != null, ct: ct)
        .OnDuplicate(Result.Failure(ErrorCodes.Duplicate, "This email or ID is already registered.")), ct);
    }

    /// <summary>
    /// Adds the configured default role when it still exists and is still non-privileged (rechecked here: its
    /// permissions may have changed since it was configured). Otherwise the member gets no role.
    /// </summary>
    private async Task<string?> AssignDefaultRoleAsync(ApplicationUser user, MemberType type, CancellationToken ct)
    {
        var config = await uow.GeneralConfigs.GetByKeyAsync(RegistrationConfigKeys.DefaultRoleFor(type), ct);
        var role = await DefaultRole.ResolveAsync(uow, normalizer, config?.Value, ct);
        if (role != null && !(await userManager.AddToRoleAsync(user, role)).Succeeded)
            throw new InvalidOperationException($"Could not add the default role {role}."); // rolls the registration back
        return role;
    }

    public Task<IReadOnlyList<PendingRegistrationDto>> GetPendingAsync(CancellationToken ct = default) => uow.LibraryMembers.GetPendingAsync(ct);

    public Task<Result> ApproveAsync(int memberId, CancellationToken ct = default) => ReviewAsync(memberId, ApprovalStatus.Approved, ct);

    public Task<Result> RejectAsync(int memberId, CancellationToken ct = default) => ReviewAsync(memberId, ApprovalStatus.Rejected, ct);

    private Task<Result> ReviewAsync(int memberId, ApprovalStatus decision, CancellationToken ct) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var member = await uow.LibraryMembers.GetForUpdateAsync(memberId, attemptCt);
            if (member is not { Approval: ApprovalStatus.Pending, IdentityUserId: not null })
                return Result.Failure(ErrorCodes.NotFound, "Pending registration not found.");
            var user = await userManager.FindByIdAsync(member.IdentityUserId);
            if (user == null || user.Status == RecordStatus.Deleted) return Result.Failure(ErrorCodes.NotFound, "The applicant's account no longer exists.");

            string? role = null;
            var claim = !string.Equals(member.Email, user.Email, StringComparison.OrdinalIgnoreCase); // see CreateAsync
            member.Approval = decision;
            if (decision == ApprovalStatus.Rejected && claim)
            {
                member.IdentityUserId = null; // unlinked, so the real owner of the ID can still register
                member.Approval = ApprovalStatus.Approved;
            }
            if (decision == ApprovalStatus.Approved)
            {
                if (!claim) member.Status = RecordStatus.Active; // a claimed record keeps the status staff set
                user.Status = RecordStatus.Active;
                if ((await userManager.UpdateAsync(user)).ToResult() is { Succeeded: false } failed) return failed;
                role = await AssignDefaultRoleAsync(user, member.MemberType, attemptCt);
            }
            await uow.AuditLogs.AddAsync(SecurityEvent.For(SecurityEvent.User, user.Id, $"Registration{decision}", role == null ? null : new { Role = role }), attemptCt);
            return Result.Success();
        }, ct: ct);
}
