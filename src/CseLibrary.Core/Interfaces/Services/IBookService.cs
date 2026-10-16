using CseLibrary.Core.DTOs;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Core.Interfaces.Services;

public interface IBookService
{
    Task<PagedResult<BookListItemDto>> GetPageAsync(PageRequest page, string? search, int? categoryId = null, int? authorId = null, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<BookDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(BookFormDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(BookFormDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);

    Task<BookCopyDto?> GetCopyAsync(int id, CancellationToken ct = default);
    Task<Result> AddCopyAsync(BookCopyDto dto, CancellationToken ct = default);
    Task<Result> UpdateCopyAsync(BookCopyDto dto, CancellationToken ct = default);
    Task<Result> DeleteCopyAsync(int id, CancellationToken ct = default);
}
