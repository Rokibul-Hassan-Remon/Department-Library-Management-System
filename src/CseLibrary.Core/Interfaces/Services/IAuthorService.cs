using CseLibrary.Core.DTOs;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Core.Interfaces.Services;

/// <summary>The Authors register: an author is created here before a book can name them.</summary>
public interface IAuthorService
{
    Task<PagedResult<AuthorListItemDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default);
    Task<AuthorFormDto?> GetAsync(int id, CancellationToken ct = default);
    /// <summary>The book form's author picker: at most 20 names containing <paramref name="term"/>.</summary>
    Task<IReadOnlyList<AuthorOptionDto>> SearchAsync(string? term, CancellationToken ct = default);
    /// <summary>Names for the given ids, in the given order (missing ids are skipped).</summary>
    Task<IReadOnlyList<AuthorOptionDto>> GetOptionsAsync(IReadOnlyList<int> ids, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(AuthorFormDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(AuthorFormDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    /// <summary>Stores a new photo (JPG/PNG/WebP, max 2 MB) or, with null, removes it; the replaced file is deleted.</summary>
    Task<Result> SetPhotoAsync(int id, ImageUpload? image, CancellationToken ct = default);
}
