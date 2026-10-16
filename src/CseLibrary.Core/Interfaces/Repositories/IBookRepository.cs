using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Paging;

namespace CseLibrary.Core.Interfaces.Repositories;

public interface IBookRepository : IRepository<Book>
{
    /// <summary>Matches title, an author's name, ISBN or a copy's accession number; optionally one category.</summary>
    Task<PagedResult<BookListItemDto>> GetPageAsync(PageRequest page, string? search, int? categoryId, CancellationToken ct = default);
    Task<BookDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    /// <summary>Tracked, with its author links, for an edit.</summary>
    Task<Book?> GetWithAuthorsForUpdateAsync(int id, CancellationToken ct = default);
    Task<bool> IsbnExistsAsync(string isbn, int? excludeId, CancellationToken ct = default);
    Task<bool> HasCopiesAsync(int id, CancellationToken ct = default);
}

public interface IBookCopyRepository : IRepository<BookCopy>
{
    Task<BookCopyDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<bool> AccessionExistsAsync(string accessionNumber, int? excludeId, CancellationToken ct = default);
    Task<BookCopy?> FindByAccessionForUpdateAsync(string accessionNumber, CancellationToken ct = default);
    /// <summary>The save fails with a concurrency conflict unless the row still has <paramref name="rowVersion"/>.</summary>
    void ExpectVersion(BookCopy copy, byte[] rowVersion);
    /// <summary>All copies (or one status), ordered by accession number, with the holder of an issued copy.</summary>
    Task<PagedResult<BookCopyDto>> GetPageAsync(PageRequest page, CopyStatus? status, CancellationToken ct = default);
    Task<IReadOnlyDictionary<CopyStatus, int>> CountByStatusAsync(CancellationToken ct = default);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<CategoryDto>> GetListAsync(CancellationToken ct = default);
    Task<Category?> FindByNameForUpdateAsync(string name, CancellationToken ct = default);
}

public interface IAuthorRepository : IRepository<Author>
{
    Task<IReadOnlyList<Author>> FindByNamesForUpdateAsync(IReadOnlyCollection<string> names, CancellationToken ct = default);
}

public interface ILibraryPolicyRepository : IRepository<LibraryPolicy>
{
    Task<IReadOnlyList<LibraryPolicyDto>> GetListAsync(CancellationToken ct = default);
    Task<LibraryPolicyDto?> GetForTypeAsync(MemberType type, CancellationToken ct = default);
}

public interface IEmailNotificationLogRepository : IRepository<EmailNotificationLog>
{
    Task<EmailNotificationLog?> FindByKeyForUpdateAsync(string idempotencyKey, CancellationToken ct = default);
    Task<PagedResult<EmailLogDto>> GetPageAsync(PageRequest page, DeliveryStatus? status, CancellationToken ct = default);
}
