using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Services.Services;

public class BookService(IUnitOfWork uow) : IBookService
{
    public Task<PagedResult<BookListItemDto>> GetPageAsync(PageRequest page, string? search, int? categoryId = null, CancellationToken ct = default) =>
        uow.Books.GetPageAsync(page, search, categoryId, ct);

    public Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default) => uow.Categories.GetListAsync(ct);

    public Task<BookDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default) => uow.Books.GetDetailsAsync(id, ct);

    // A transaction, because one save may add the book, new authors, a new category and the links.
    public Task<Result<int>> CreateAsync(BookFormDto dto, CancellationToken ct = default)
    {
        return uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var isbn = Clean(dto.Isbn);
            if (isbn != null && await uow.Books.IsbnExistsAsync(isbn, null, attemptCt)) return Result<int>.Failure(ErrorCodes.Duplicate, IsbnExists);

            var book = new Book();
            await ApplyAsync(book, dto, isbn, attemptCt);
            await uow.Books.AddAsync(book, attemptCt); // HiLo: Id is known here (D-29)
            return Result<int>.Success(book.Id);
        }, ct: ct).OnDuplicate(Result<int>.Failure(ErrorCodes.Duplicate, DuplicateOnSave));
    }

    public Task<Result> UpdateAsync(BookFormDto dto, CancellationToken ct = default) =>
        uow.ExecuteInTransactionAsync(async attemptCt =>
        {
            var book = await uow.Books.GetWithAuthorsForUpdateAsync(dto.Id, attemptCt);
            if (book == null) return BookNotFound();
            var isbn = Clean(dto.Isbn);
            if (isbn != null && await uow.Books.IsbnExistsAsync(isbn, dto.Id, attemptCt)) return Result.Failure(ErrorCodes.Duplicate, IsbnExists);

            await ApplyAsync(book, dto, isbn, attemptCt);
            return Result.Success();
        }, ct: ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, DuplicateOnSave));

    /// <summary>Soft delete; refused while copies exist, so copy (and lending) history keeps its title.</summary>
    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var book = await uow.Books.GetForUpdateAsync(id, ct);
        if (book == null) return BookNotFound();
        if (await uow.Books.HasCopiesAsync(id, ct))
            return Result.Failure(ErrorCodes.InUse, "Delete or retire this book's copies first.");

        uow.Books.Remove(book);
        return await uow.SaveAsync(ct);
    }

    public Task<BookCopyDto?> GetCopyAsync(int id, CancellationToken ct = default) => uow.BookCopies.GetDetailsAsync(id, ct);

    public async Task<Result> AddCopyAsync(BookCopyDto dto, CancellationToken ct = default)
    {
        if (await uow.Books.GetForUpdateAsync(dto.BookId, ct) == null) return BookNotFound();
        if (dto.CopyStatus == CopyStatus.Issued) return IssuedByLendingOnly();
        var accession = dto.AccessionNumber.Trim();
        if (await uow.BookCopies.AccessionExistsAsync(accession, null, ct)) return AccessionExists();

        await uow.BookCopies.AddAsync(new BookCopy
        {
            BookId = dto.BookId, AccessionNumber = accession, ShelfLocation = Clean(dto.ShelfLocation),
            AcquisitionDate = dto.AcquisitionDate, CopyStatus = dto.CopyStatus, Notes = Clean(dto.Notes)
        }, ct);
        return await uow.SaveAsync(ct).OnDuplicate(AccessionExists());
    }

    public async Task<Result> UpdateCopyAsync(BookCopyDto dto, CancellationToken ct = default)
    {
        var copy = await uow.BookCopies.GetForUpdateAsync(dto.Id, ct);
        if (copy == null) return CopyNotFound();
        if (dto.RowVersion is { Length: > 0 }) uow.BookCopies.ExpectVersion(copy, dto.RowVersion); // refuse a stale form
        // Issued is owned by lending: a catalog edit can neither set it nor clear it.
        if ((dto.CopyStatus == CopyStatus.Issued) != (copy.CopyStatus == CopyStatus.Issued)) return IssuedByLendingOnly();
        var accession = dto.AccessionNumber.Trim();
        if (await uow.BookCopies.AccessionExistsAsync(accession, dto.Id, ct)) return AccessionExists();

        copy.AccessionNumber = accession;
        copy.ShelfLocation = Clean(dto.ShelfLocation);
        copy.AcquisitionDate = dto.AcquisitionDate;
        copy.CopyStatus = dto.CopyStatus;
        copy.Notes = Clean(dto.Notes);
        return await uow.SaveAsync(ct).OnDuplicate(AccessionExists())
            .OnConflict(Result.Failure(ErrorCodes.InvalidState, "This copy was changed meanwhile (for example issued). Reload and try again."));
    }

    public async Task<Result> DeleteCopyAsync(int id, CancellationToken ct = default)
    {
        var copy = await uow.BookCopies.GetForUpdateAsync(id, ct);
        if (copy == null) return CopyNotFound();
        if (copy.CopyStatus == CopyStatus.Issued) return Result.Failure(ErrorCodes.InUse, "An issued copy cannot be deleted.");

        uow.BookCopies.Remove(copy);
        return await uow.SaveAsync(ct).OnConflict(Result.Failure(ErrorCodes.InvalidState, "This copy was just changed. Reload and try again."));
    }

    private async Task ApplyAsync(Book book, BookFormDto dto, string? isbn, CancellationToken ct)
    {
        book.Isbn = isbn;
        book.Title = dto.Title.Trim();
        book.Publisher = Clean(dto.Publisher);
        book.Edition = Clean(dto.Edition);
        book.PublicationYear = dto.PublicationYear;
        book.Description = Clean(dto.Description);

        var categoryName = Clean(dto.Category);
        if (categoryName == null) book.CategoryId = null;
        else
        {
            var category = await uow.Categories.FindByNameForUpdateAsync(categoryName, ct);
            if (category == null) await uow.Categories.AddAsync(category = new Category { Name = categoryName }, ct);
            book.CategoryId = category.Id; // HiLo: known before save
        }

        // Authors: split, trim, de-duplicate (case-insensitive), find or create, keep the typed order.
        var names = (dto.Authors ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(n => n.Length > 150 ? n[..150] : n).DistinctBy(n => n.ToUpperInvariant()).ToList();
        var existing = await uow.Authors.FindByNamesForUpdateAsync(names, ct);
        book.BookAuthors.Clear();
        for (var i = 0; i < names.Count; i++)
        {
            var author = existing.FirstOrDefault(a => string.Equals(a.Name, names[i], StringComparison.OrdinalIgnoreCase));
            if (author == null) await uow.Authors.AddAsync(author = new Author { Name = names[i] }, ct);
            book.BookAuthors.Add(new BookAuthor { AuthorId = author.Id, Order = (byte)Math.Min(i, byte.MaxValue) });
        }
    }

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private const string IsbnExists = "A book with this ISBN already exists.";
    private const string DuplicateOnSave = "The ISBN, an author or the category was just added by someone else. Please save again.";
    private static Result AccessionExists() => Result.Failure(ErrorCodes.Duplicate, "This accession number is already used.");
    private static Result IssuedByLendingOnly() =>
        Result.Failure(ErrorCodes.InvalidState, "The Issued status is set only by issuing and returning a copy.");
    private static Result BookNotFound() => Result.Failure(ErrorCodes.NotFound, "Book not found");
    private static Result CopyNotFound() => Result.Failure(ErrorCodes.NotFound, "Copy not found");
}
