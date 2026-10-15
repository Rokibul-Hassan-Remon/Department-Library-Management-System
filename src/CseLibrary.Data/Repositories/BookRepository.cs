using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Core.Paging;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

public class BookRepository(ApplicationDbContext db) : Repository<Book>(db), IBookRepository
{
    public async Task<PagedResult<BookListItemDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default)
    {
        var books = Db.Books.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            // ponytail: LIKE '%s%' scans; full-text index when the catalog is large
            books = books.Where(b => b.Title.Contains(s) || (b.Authors != null && b.Authors.Contains(s))
                                     || b.Isbn == s || b.Copies.Any(c => c.AccessionNumber == s));
        }

        var total = await books.CountAsync(ct);
        page = page.ClampTo(total);
        var items = await books.OrderBy(b => b.Title).ThenBy(b => b.Id).Skip(page.Skip).Take(page.PageSize)
            .Select(b => new BookListItemDto
            {
                Id = b.Id, Isbn = b.Isbn, Title = b.Title, Authors = b.Authors, Category = b.Category,
                TotalCopies = b.Copies.Count(),
                AvailableCopies = b.Copies.Count(c => c.CopyStatus == CopyStatus.Available)
            }).ToListAsync(ct);
        return new PagedResult<BookListItemDto>(items, total, page.Page, page.PageSize);
    }

    public Task<BookDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default) =>
        Db.Books.Where(b => b.Id == id).Select(b => new BookDetailsDto
        {
            Id = b.Id, Isbn = b.Isbn, Title = b.Title, Authors = b.Authors, Publisher = b.Publisher, Edition = b.Edition,
            PublicationYear = b.PublicationYear, Category = b.Category, Description = b.Description,
            Copies = b.Copies.OrderBy(c => c.AccessionNumber).Select(c => new BookCopyDto
            {
                Id = c.Id, BookId = c.BookId, AccessionNumber = c.AccessionNumber, ShelfLocation = c.ShelfLocation,
                AcquisitionDate = c.AcquisitionDate, CopyStatus = c.CopyStatus, Notes = c.Notes
            }).ToList()
        }).FirstOrDefaultAsync(ct);

    public Task<bool> IsbnExistsAsync(string isbn, int? excludeId, CancellationToken ct = default) =>
        Db.Books.AnyAsync(b => b.Isbn == isbn && b.Id != excludeId, ct);

    public Task<bool> HasCopiesAsync(int id, CancellationToken ct = default) =>
        Db.BookCopies.AnyAsync(c => c.BookId == id, ct);
}

public class BookCopyRepository(ApplicationDbContext db) : Repository<BookCopy>(db), IBookCopyRepository
{
    public Task<BookCopyDto?> GetDetailsAsync(int id, CancellationToken ct = default) =>
        Db.BookCopies.Where(c => c.Id == id).Select(c => new BookCopyDto
        {
            Id = c.Id, BookId = c.BookId, BookTitle = c.Book.Title, AccessionNumber = c.AccessionNumber,
            ShelfLocation = c.ShelfLocation, AcquisitionDate = c.AcquisitionDate, CopyStatus = c.CopyStatus, Notes = c.Notes
        }).FirstOrDefaultAsync(ct);

    public Task<bool> AccessionExistsAsync(string accessionNumber, int? excludeId, CancellationToken ct = default) =>
        Db.BookCopies.AnyAsync(c => c.AccessionNumber == accessionNumber && c.Id != excludeId, ct);
}
