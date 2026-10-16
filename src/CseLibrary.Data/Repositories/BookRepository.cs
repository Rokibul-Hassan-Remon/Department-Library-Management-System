using System.Linq.Expressions;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Core.Paging;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

public class BookRepository(ApplicationDbContext db) : Repository<Book>(db), IBookRepository
{
    public async Task<PagedResult<BookListItemDto>> GetPageAsync(PageRequest page, string? search, int? categoryId, CancellationToken ct = default)
    {
        var books = Db.Books.AsQueryable();
        if (categoryId != null) books = books.Where(b => b.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            // ponytail: LIKE '%s%' scans; full-text index when the catalog is large
            books = books.Where(b => b.Title.Contains(s) || b.BookAuthors.Any(ba => ba.Author.Name.Contains(s))
                                     || b.Isbn == s || b.Copies.Any(c => c.AccessionNumber == s));
        }

        var total = await books.CountAsync(ct);
        page = page.ClampTo(total);
        var items = await books.OrderBy(b => b.Title).ThenBy(b => b.Id).Skip(page.Skip).Take(page.PageSize)
            .Select(b => new BookListItemDto
            {
                Id = b.Id, Isbn = b.Isbn, Title = b.Title, Category = b.Category!.Name,
                AuthorNames = b.BookAuthors.OrderBy(ba => ba.Order).Select(ba => ba.Author.Name).ToList(),
                TotalCopies = b.Copies.Count(),
                AvailableCopies = b.Copies.Count(c => c.CopyStatus == CopyStatus.Available)
            }).ToListAsync(ct);
        return new PagedResult<BookListItemDto>(items, total, page.Page, page.PageSize);
    }

    public async Task<BookDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var copyDto = BookCopyRepository.ToDto(Db);
        var row = await Db.Books.Where(b => b.Id == id).Select(b => new
        {
            Book = new BookDetailsDto
            {
                Id = b.Id, Isbn = b.Isbn, Title = b.Title, Publisher = b.Publisher, Edition = b.Edition,
                PublicationYear = b.PublicationYear, Category = b.Category!.Name, Description = b.Description,
                Copies = b.Copies.AsQueryable().OrderBy(c => c.AccessionNumber).Select(copyDto).ToList()
            },
            Authors = b.BookAuthors.OrderBy(ba => ba.Order).Select(ba => ba.Author.Name).ToList()
        }).FirstOrDefaultAsync(ct);
        if (row == null) return null;
        row.Book.Authors = string.Join(", ", row.Authors);
        return row.Book;
    }

    public Task<Book?> GetWithAuthorsForUpdateAsync(int id, CancellationToken ct = default) =>
        Db.Books.Include(b => b.BookAuthors).FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<bool> IsbnExistsAsync(string isbn, int? excludeId, CancellationToken ct = default) =>
        Db.Books.AnyAsync(b => b.Isbn == isbn && b.Id != excludeId, ct);

    public Task<bool> HasCopiesAsync(int id, CancellationToken ct = default) =>
        Db.BookCopies.AnyAsync(c => c.BookId == id, ct);
}

public class BookCopyRepository(ApplicationDbContext db) : Repository<BookCopy>(db), IBookCopyRepository
{
    /// <summary>A copy row with its title and the member holding it if issued; shared by book details and the copies report.</summary>
    internal static Expression<Func<BookCopy, BookCopyDto>> ToDto(ApplicationDbContext db) => c => new BookCopyDto
    {
        Id = c.Id, BookId = c.BookId, BookTitle = c.Book.Title, AccessionNumber = c.AccessionNumber, ShelfLocation = c.ShelfLocation,
        AcquisitionDate = c.AcquisitionDate, CopyStatus = c.CopyStatus, Notes = c.Notes,
        HeldBy = db.Loans.Where(l => l.BookCopyId == c.Id && l.TransactionStatus == LendingStatus.Issued)
            .Select(l => l.LibraryMember.MemberCode + " " + l.LibraryMember.FullName).FirstOrDefault()
    };

    public Task<BookCopyDto?> GetDetailsAsync(int id, CancellationToken ct = default) =>
        Db.BookCopies.Where(c => c.Id == id).Select(c => new BookCopyDto
        {
            Id = c.Id, BookId = c.BookId, BookTitle = c.Book.Title, AccessionNumber = c.AccessionNumber,
            ShelfLocation = c.ShelfLocation, AcquisitionDate = c.AcquisitionDate, CopyStatus = c.CopyStatus, Notes = c.Notes,
            RowVersion = c.RowVersion
        }).FirstOrDefaultAsync(ct);

    public Task<bool> AccessionExistsAsync(string accessionNumber, int? excludeId, CancellationToken ct = default) =>
        Db.BookCopies.AnyAsync(c => c.AccessionNumber == accessionNumber && c.Id != excludeId, ct);

    public Task<BookCopy?> FindByAccessionForUpdateAsync(string accessionNumber, CancellationToken ct = default) =>
        Db.BookCopies.FirstOrDefaultAsync(c => c.AccessionNumber == accessionNumber, ct);

    public void ExpectVersion(BookCopy copy, byte[] rowVersion) =>
        Db.Entry(copy).Property(c => c.RowVersion).OriginalValue = rowVersion;

    public async Task<PagedResult<BookCopyDto>> GetPageAsync(PageRequest page, CopyStatus? status, CancellationToken ct = default)
    {
        var copies = Db.BookCopies.Where(c => status == null || c.CopyStatus == status);
        var total = await copies.CountAsync(ct);
        page = page.ClampTo(total);
        var items = await copies.OrderBy(c => c.AccessionNumber).ThenBy(c => c.Id).Skip(page.Skip).Take(page.PageSize)
            .Select(ToDto(Db)).ToListAsync(ct);
        return new PagedResult<BookCopyDto>(items, total, page.Page, page.PageSize);
    }

    public async Task<IReadOnlyDictionary<CopyStatus, int>> CountByStatusAsync(CancellationToken ct = default) =>
        await Db.BookCopies.GroupBy(c => c.CopyStatus).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(g => g.Key, g => g.Count, ct);
}

public class CategoryRepository(ApplicationDbContext db) : Repository<Category>(db), ICategoryRepository
{
    public async Task<IReadOnlyList<CategoryDto>> GetListAsync(CancellationToken ct = default) =>
        await Db.Categories.OrderBy(c => c.Name).Select(c => new CategoryDto(c.Id, c.Name)).ToListAsync(ct);

    public Task<Category?> FindByNameForUpdateAsync(string name, CancellationToken ct = default) =>
        Db.Categories.FirstOrDefaultAsync(c => c.Name == name, ct);
}

public class AuthorRepository(ApplicationDbContext db) : Repository<Author>(db), IAuthorRepository
{
    public async Task<IReadOnlyList<Author>> FindByNamesForUpdateAsync(IReadOnlyCollection<string> names, CancellationToken ct = default) =>
        await Db.Authors.Where(a => names.Contains(a.Name)).ToListAsync(ct);
}

public class LibraryPolicyRepository(ApplicationDbContext db) : Repository<LibraryPolicy>(db), ILibraryPolicyRepository
{
    public async Task<IReadOnlyList<LibraryPolicyDto>> GetListAsync(CancellationToken ct = default) =>
        await ToDto(Db.LibraryPolicies.OrderBy(p => p.MemberCategory)).ToListAsync(ct);

    public Task<LibraryPolicyDto?> GetForTypeAsync(MemberType type, CancellationToken ct = default) =>
        ToDto(Db.LibraryPolicies.Where(p => p.MemberCategory == type)).FirstOrDefaultAsync(ct);

    private static IQueryable<LibraryPolicyDto> ToDto(IQueryable<LibraryPolicy> q) => q.Select(p => new LibraryPolicyDto
    {
        Id = p.Id, MemberCategory = p.MemberCategory, MaxActiveLoans = p.MaxActiveLoans, LoanDays = p.LoanDays, MaxRenewals = p.MaxRenewals
    });
}

public class EmailNotificationLogRepository(ApplicationDbContext db) : Repository<EmailNotificationLog>(db), IEmailNotificationLogRepository
{
    public async Task<IReadOnlyList<EmailNotificationLog>> FindByKeysForUpdateAsync(IReadOnlyCollection<string> idempotencyKeys, CancellationToken ct = default) =>
        await Db.EmailNotificationLogs.Where(e => idempotencyKeys.Contains(e.IdempotencyKey)).ToListAsync(ct);

    public async Task<PagedResult<EmailLogDto>> GetPageAsync(PageRequest page, DeliveryStatus? status, CancellationToken ct = default)
    {
        var emails = Db.EmailNotificationLogs.Where(e => status == null || e.DeliveryStatus == status);
        var total = await emails.CountAsync(ct);
        page = page.ClampTo(total);
        var items = await emails.OrderByDescending(e => e.CreatedAt).ThenByDescending(e => e.Id).Skip(page.Skip).Take(page.PageSize)
            .Select(e => new EmailLogDto(e.CreatedAt, e.EmailType, e.Recipient, e.Subject, e.DeliveryStatus, e.Attempts, e.SentAtUtc, e.LastError))
            .ToListAsync(ct);
        return new PagedResult<EmailLogDto>(items, total, page.Page, page.PageSize);
    }
}
