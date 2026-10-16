using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Services.Services;

public class LibraryMemberService(IUnitOfWork uow) : ILibraryMemberService
{
    public Task<PagedResult<LibraryMemberDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default) =>
        uow.LibraryMembers.GetPageAsync(page, search, null, ct);

    public Task<LibraryMemberDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default) => uow.LibraryMembers.GetDetailsAsync(id, ct);

    public async Task<LibraryMemberDetailsDto?> GetForUserAsync(string userId, CancellationToken ct = default) =>
        await uow.LibraryMembers.FindIdByUserIdAsync(userId, ct) is { } id ? await uow.LibraryMembers.GetDetailsAsync(id, ct) : null;

    public async Task<Result<int>> CreateAsync(LibraryMemberDto dto, CancellationToken ct = default)
    {
        var code = dto.MemberCode.Trim();
        if (await uow.LibraryMembers.CodeExistsAsync(code, null, ct)) return Result<int>.Failure(ErrorCodes.Duplicate, CodeExists);

        var member = new LibraryMember();
        Apply(member, dto, code);
        await uow.LibraryMembers.AddAsync(member, ct);
        var saved = await uow.SaveAsync(ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, CodeExists));
        return saved.Succeeded ? Result<int>.Success(member.Id) : Result<int>.Failure(saved.ErrorCode!, saved.Error!);
    }

    public async Task<Result> UpdateAsync(LibraryMemberDto dto, CancellationToken ct = default)
    {
        var member = await uow.LibraryMembers.GetForUpdateAsync(dto.Id, ct);
        if (member == null) return NotFound();
        var code = dto.MemberCode.Trim();
        if (await uow.LibraryMembers.CodeExistsAsync(code, dto.Id, ct)) return Result.Failure(ErrorCodes.Duplicate, CodeExists);

        Apply(member, dto, code);
        return await uow.SaveAsync(ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, CodeExists));
    }

    /// <summary>Soft delete; refused while the member holds books. Loan history keeps the row.</summary>
    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var member = await uow.LibraryMembers.GetForUpdateAsync(id, ct);
        if (member == null) return NotFound();
        if (await uow.Loans.HasActiveAsync(id, ct)) return Result.Failure(ErrorCodes.InUse, "The member still has books to return.");

        uow.LibraryMembers.Remove(member);
        return await uow.SaveAsync(ct);
    }

    private static void Apply(LibraryMember member, LibraryMemberDto dto, string code)
    {
        member.MemberCode = code;
        member.FullName = dto.FullName.Trim();
        member.MemberType = dto.MemberType;
        member.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        member.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        // A Pending or Rejected registration becomes Active only through review (D-54), never through this edit.
        member.Status = dto.IsActive && member.Approval == ApprovalStatus.Approved ? RecordStatus.Active : RecordStatus.Inactive;
    }

    private const string CodeExists = "A member with this code already exists.";
    private static Result NotFound() => Result.Failure(ErrorCodes.NotFound, "Library member not found");
}
