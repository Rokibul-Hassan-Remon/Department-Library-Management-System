using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Services.Services;

public class MemberService(IUnitOfWork uow) : IMemberService
{
    public Task<PagedResult<MemberDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default) =>
        uow.Members.GetPageAsync(page, search, ct);

    public Task<MemberDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default) => uow.Members.GetDetailsAsync(id, ct);

    public async Task<MemberDetailsDto?> GetForUserAsync(string userId, CancellationToken ct = default) =>
        await uow.Members.FindIdByUserIdAsync(userId, ct) is { } id ? await uow.Members.GetDetailsAsync(id, ct) : null;

    public async Task<Result<int>> CreateAsync(MemberDto dto, CancellationToken ct = default)
    {
        var code = dto.MemberCode.Trim();
        if (await uow.Members.CodeExistsAsync(code, null, ct)) return Result<int>.Failure(ErrorCodes.Duplicate, CodeExists);

        var member = new Member();
        Apply(member, dto, code);
        await uow.Members.AddAsync(member, ct);
        var saved = await uow.SaveAsync(ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, CodeExists));
        return saved.Succeeded ? Result<int>.Success(member.Id) : Result<int>.Failure(saved.ErrorCode!, saved.Error!);
    }

    public async Task<Result> UpdateAsync(MemberDto dto, CancellationToken ct = default)
    {
        var member = await uow.Members.GetForUpdateAsync(dto.Id, ct);
        if (member == null) return NotFound();
        var code = dto.MemberCode.Trim();
        if (await uow.Members.CodeExistsAsync(code, dto.Id, ct)) return Result.Failure(ErrorCodes.Duplicate, CodeExists);

        Apply(member, dto, code);
        return await uow.SaveAsync(ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, CodeExists));
    }

    /// <summary>Soft delete; refused while the member holds books. Loan history keeps the row.</summary>
    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var member = await uow.Members.GetForUpdateAsync(id, ct);
        if (member == null) return NotFound();
        if (await uow.Loans.HasActiveAsync(id, ct)) return Result.Failure(ErrorCodes.InUse, "The member still has books to return.");

        uow.Members.Remove(member);
        return await uow.SaveAsync(ct);
    }

    private static void Apply(Member member, MemberDto dto, string code)
    {
        member.MemberCode = code;
        member.FullName = dto.FullName.Trim();
        member.MemberType = dto.MemberType;
        member.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        member.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        member.Status = dto.IsActive ? RecordStatus.Active : RecordStatus.Inactive;
    }

    private const string CodeExists = "A member with this code already exists.";
    private static Result NotFound() => Result.Failure(ErrorCodes.NotFound, "Member not found");
}
