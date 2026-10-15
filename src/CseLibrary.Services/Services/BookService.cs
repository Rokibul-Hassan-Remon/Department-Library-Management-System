using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Services.Services;

public class BookService(IUnitOfWork uow) : IBookService
{
    public Task<PagedResult<BookListItemDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default) =>
        uow.Books.GetPageAsync(page, search, ct);

    public Task<BookDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default) => uow.Books.GetDetailsAsync(id, ct);

    public async Task<Result<int>> CreateAsync(BookFormDto dto, CancellationToken ct = default)
    {
        var isbn = Clean(dto.Isbn);
        if (isbn != null && await uow.Books.IsbnExistsAsync(isbn, null, ct)) return Result<int>.Failure(ErrorCodes.Duplicate, IsbnExists);

        var book = new Book();
        Apply(book, dto, isbn);
        await uow.Books.AddAsync(book, ct); // HiLo: Id is known here (D-29)
        var saved = await uow.SaveAsync(ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, IsbnExists));
        return saved.Succeeded ? Result<int>.Success(book.Id) : Result<int>.Failure(saved.ErrorCode!, saved.Error!);
    }

    public async Task<Result> UpdateAsync(BookFormDto dto, CancellationToken ct = default)
    {
        var book = await uow.Books.GetForUpdateAsync(dto.Id, ct);
        if (book == null) return BookNotFound();
        var isbn = Clean(dto.Isbn);
        if (isbn != null && await uow.Books.IsbnExistsAsync(isbn, dto.Id, ct)) return Result.Failure(ErrorCodes.Duplicate, IsbnExists);

        Apply(book, dto, isbn);
        return await uow.SaveAsync(ct).OnDuplicate(Result.Failure(ErrorCodes.Duplicate, IsbnExists));
    }

    /// <summary>Soft delete; refused while copies exist, so copy (and later lending) history keeps its title.</summary>
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
        // Issued is owned by lending: a catalog edit can neither set it nor clear it.
        if ((dto.CopyStatus == CopyStatus.Issued) != (copy.CopyStatus == CopyStatus.Issued)) return IssuedByLendingOnly();
        var accession = dto.AccessionNumber.Trim();
        if (await uow.BookCopies.AccessionExistsAsync(accession, dto.Id, ct)) return AccessionExists();

        copy.AccessionNumber = accession;
        copy.ShelfLocation = Clean(dto.ShelfLocation);
        copy.AcquisitionDate = dto.AcquisitionDate;
        copy.CopyStatus = dto.CopyStatus;
        copy.Notes = Clean(dto.Notes);
        return await uow.SaveAsync(ct).OnDuplicate(AccessionExists());
    }

    public async Task<Result> DeleteCopyAsync(int id, CancellationToken ct = default)
    {
        var copy = await uow.BookCopies.GetForUpdateAsync(id, ct);
        if (copy == null) return CopyNotFound();
        if (copy.CopyStatus == CopyStatus.Issued) return Result.Failure(ErrorCodes.InUse, "An issued copy cannot be deleted.");

        uow.BookCopies.Remove(copy);
        return await uow.SaveAsync(ct);
    }

    private static void Apply(Book book, BookFormDto dto, string? isbn)
    {
        book.Isbn = isbn;
        book.Title = dto.Title.Trim();
        book.Authors = Clean(dto.Authors);
        book.Publisher = Clean(dto.Publisher);
        book.Edition = Clean(dto.Edition);
        book.PublicationYear = dto.PublicationYear;
        book.Category = Clean(dto.Category);
        book.Description = Clean(dto.Description);
    }

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private const string IsbnExists = "A book with this ISBN already exists.";
    private static Result AccessionExists() => Result.Failure(ErrorCodes.Duplicate, "This accession number is already used.");
    private static Result IssuedByLendingOnly() =>
        Result.Failure(ErrorCodes.InvalidState, "The Issued status is set only by issuing and returning a copy.");
    private static Result BookNotFound() => Result.Failure(ErrorCodes.NotFound, "Book not found");
    private static Result CopyNotFound() => Result.Failure(ErrorCodes.NotFound, "Copy not found");
}
