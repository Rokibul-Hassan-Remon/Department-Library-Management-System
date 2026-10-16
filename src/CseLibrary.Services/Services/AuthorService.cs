using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Services.Services;

public class AuthorService(IUnitOfWork uow) : IAuthorService
{
    public Task<PagedResult<AuthorListItemDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default) =>
        uow.Authors.GetPageAsync(page, search, ct);

    public Task<AuthorFormDto?> GetAsync(int id, CancellationToken ct = default) => uow.Authors.GetFormAsync(id, ct);

    public Task<IReadOnlyList<AuthorOptionDto>> SearchAsync(string? term, CancellationToken ct = default) =>
        uow.Authors.SearchAsync(term, 20, ct);

    public async Task<IReadOnlyList<AuthorOptionDto>> GetOptionsAsync(IReadOnlyList<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return [];
        var found = (await uow.Authors.GetOptionsAsync(ids, ct)).ToDictionary(a => a.Id);
        return ids.Distinct().Where(found.ContainsKey).Select(id => found[id]).ToList();
    }

    public async Task<Result<int>> CreateAsync(AuthorFormDto dto, CancellationToken ct = default)
    {
        var name = dto.Name.Trim();
        if (await uow.Authors.NameExistsAsync(name, null, ct)) return Result<int>.Failure(ErrorCodes.Duplicate, NameExists);

        var author = new Author { Name = name };
        await uow.Authors.AddAsync(author, ct); // HiLo: Id is known here (D-29)
        var saved = await uow.SaveAsync(ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, NameExists));
        return saved.Succeeded ? Result<int>.Success(author.Id) : Result<int>.Failure(saved.ErrorCode!, saved.Error!);
    }

    public async Task<Result> UpdateAsync(AuthorFormDto dto, CancellationToken ct = default)
    {
        var author = await uow.Authors.GetForUpdateAsync(dto.Id, ct);
        if (author == null) return NotFound();
        var name = dto.Name.Trim();
        if (await uow.Authors.NameExistsAsync(name, dto.Id, ct)) return Result.Failure(ErrorCodes.Duplicate, NameExists);

        author.Name = name;
        return await uow.SaveAsync(ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, NameExists));
    }

    /// <summary>Soft delete; refused while a book names the author, so no book loses an author silently.</summary>
    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var author = await uow.Authors.GetForUpdateAsync(id, ct);
        if (author == null) return NotFound();
        if (await uow.Authors.HasBooksAsync(id, ct))
            return Result.Failure(ErrorCodes.InUse, "This author has books. Remove the author from those books first.");

        uow.Authors.Remove(author);
        return await uow.SaveAsync(ct);
    }

    private const string NameExists = "An author with this name already exists.";
    private static Result NotFound() => Result.Failure(ErrorCodes.NotFound, "Author not found");
}
