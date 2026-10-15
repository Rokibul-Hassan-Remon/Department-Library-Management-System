using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Paging;

namespace CseLibrary.Core.Interfaces.Repositories;

public interface IBookRepository : IRepository<Book>
{
    /// <summary>Matches title, authors, ISBN or a copy's accession number.</summary>
    Task<PagedResult<BookListItemDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default);
    Task<BookDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<bool> IsbnExistsAsync(string isbn, int? excludeId, CancellationToken ct = default);
    Task<bool> HasCopiesAsync(int id, CancellationToken ct = default);
}

public interface IBookCopyRepository : IRepository<BookCopy>
{
    Task<BookCopyDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<bool> AccessionExistsAsync(string accessionNumber, int? excludeId, CancellationToken ct = default);
}
