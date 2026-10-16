using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;
using CseLibrary.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

/// <summary>Phase 1 catalog: book titles and physical copies (FR-M15-01, archive FR-02/FR-03).</summary>
[Collection(AppCollection.Name)]
public class CatalogTests(AppFactory app)
{
    private static string Unique(string prefix) => prefix + Guid.NewGuid().ToString("N")[..10];

    [Fact]
    public async Task Copies_are_counted_per_title_and_found_by_accession_number()
    {
        await app.WithServiceAsync(async sp =>
        {
            var books = sp.GetRequiredService<IBookService>();
            var id = (await books.CreateAsync(new BookFormDto { Title = Unique("Algorithms "), Isbn = Unique("") })).Value;
            var acc1 = Unique("ACC");
            Assert.True((await books.AddCopyAsync(new BookCopyDto { BookId = id, AccessionNumber = acc1 })).Succeeded);
            Assert.True((await books.AddCopyAsync(new BookCopyDto { BookId = id, AccessionNumber = Unique("ACC"), CopyStatus = CopyStatus.Damaged })).Succeeded);

            var details = (await books.GetDetailsAsync(id))!;
            Assert.Equal((2, 1, 0, 1), (details.Copies.Count, details.AvailableCopies, details.IssuedCopies, details.UnavailableCopies));

            var found = Assert.Single((await books.GetPageAsync(new PageRequest(), acc1)).Items);
            Assert.Equal((id, 2, 1), (found.Id, found.TotalCopies, found.AvailableCopies));
        });
    }

    [Fact] // database model M15: Authors + BookAuthors, Categories; authors are registered before a book names them
    public async Task Books_name_registered_authors_in_order_and_an_author_with_books_is_not_deleted()
    {
        await app.WithServiceAsync(async sp =>
        {
            var (books, authors) = (sp.GetRequiredService<IBookService>(), sp.GetRequiredService<IAuthorService>());
            var (knuth, coName, category) = (Unique("Knuth "), Unique("Co "), Unique("Algorithms "));
            var knuthId = (await authors.CreateAsync(new AuthorFormDto { Name = " " + knuth + " " })).Value;
            Assert.Equal(ErrorCodes.Duplicate, (await authors.CreateAsync(new AuthorFormDto { Name = knuth.ToUpperInvariant() })).ErrorCode);
            var coId = (await authors.CreateAsync(new AuthorFormDto { Name = coName })).Value;

            var first = (await books.CreateAsync(new BookFormDto { Title = "TAOCP 1", AuthorIds = [coId, knuthId, coId], Category = category })).Value;
            var second = (await books.CreateAsync(new BookFormDto { Title = "TAOCP 2", AuthorIds = [knuthId], Category = category })).Value;
            Assert.Equal(new[] { coId, knuthId }, (await books.GetDetailsAsync(first))!.AuthorIds); // de-duplicated, chosen order kept
            Assert.Equal(ErrorCodes.NotFound, (await books.CreateAsync(new BookFormDto { Title = "X", AuthorIds = [int.MaxValue] })).ErrorCode); // unregistered

            var categoryId = Assert.Single(await books.GetCategoriesAsync(), c => c.Name == category).Id; // one row for both books
            Assert.Equal([first, second], (await books.GetPageAsync(new PageRequest(1, 100), knuth, categoryId)).Items.Select(b => b.Id).Order());
            Assert.Equal([first, second], (await books.GetPageAsync(new PageRequest(1, 100), null, authorId: knuthId)).Items.Select(b => b.Id).Order());
            Assert.Equal(2, Assert.Single((await authors.GetPageAsync(new PageRequest(), knuth)).Items).BookCount);
            Assert.Equal(knuthId, Assert.Single(await authors.SearchAsync(knuth)).Id); // the book form picker

            Assert.True((await books.UpdateAsync(new BookFormDto { Id = second, Title = "TAOCP 2", AuthorIds = [coId] })).Succeeded);
            var updated = (await books.GetDetailsAsync(second))!;
            Assert.Equal((coName, (string?)null), (updated.Authors, updated.Category));

            Assert.Equal(ErrorCodes.InUse, (await authors.DeleteAsync(knuthId)).ErrorCode); // still named by TAOCP 1
            Assert.True((await books.DeleteAsync(first)).Succeeded);
            Assert.True((await authors.DeleteAsync(knuthId)).Succeeded); // a deleted book does not block
            Assert.Null(await authors.GetAsync(knuthId));
        });
    }

    [Fact]
    public async Task Duplicate_isbn_and_accession_number_are_refused()
    {
        await app.WithServiceAsync(async sp =>
        {
            var books = sp.GetRequiredService<IBookService>();
            var isbn = Unique("");
            var id = (await books.CreateAsync(new BookFormDto { Title = "A", Isbn = isbn })).Value;
            Assert.Equal(ErrorCodes.Duplicate, (await books.CreateAsync(new BookFormDto { Title = "B", Isbn = " " + isbn })).ErrorCode);
            Assert.True((await books.CreateAsync(new BookFormDto { Title = "No ISBN 1" })).Succeeded); // many books may have no ISBN
            Assert.True((await books.CreateAsync(new BookFormDto { Title = "No ISBN 2", Isbn = "  " })).Succeeded);

            var acc = Unique("ACC");
            Assert.True((await books.AddCopyAsync(new BookCopyDto { BookId = id, AccessionNumber = acc })).Succeeded);
            Assert.Equal(ErrorCodes.Duplicate, (await books.AddCopyAsync(new BookCopyDto { BookId = id, AccessionNumber = acc })).ErrorCode);
        });
    }

    [Fact]
    public async Task Issued_status_is_owned_by_lending_and_a_title_with_copies_is_not_deleted()
    {
        var id = 0;
        await app.WithServiceAsync(async sp =>
        {
            var books = sp.GetRequiredService<IBookService>();
            id = (await books.CreateAsync(new BookFormDto { Title = Unique("T ") })).Value;
            Assert.Equal(ErrorCodes.InvalidState,
                (await books.AddCopyAsync(new BookCopyDto { BookId = id, AccessionNumber = Unique("ACC"), CopyStatus = CopyStatus.Issued })).ErrorCode);

            Assert.True((await books.AddCopyAsync(new BookCopyDto { BookId = id, AccessionNumber = Unique("ACC") })).Succeeded);
            var copy = (await books.GetDetailsAsync(id))!.Copies.Single();
            copy.CopyStatus = CopyStatus.Issued;
            Assert.Equal(ErrorCodes.InvalidState, (await books.UpdateCopyAsync(copy)).ErrorCode);

            Assert.Equal(ErrorCodes.InUse, (await books.DeleteAsync(id)).ErrorCode);
            Assert.True((await books.DeleteCopyAsync(copy.Id)).Succeeded);
        });
        await app.WithServiceAsync(async sp => // a new request scope, as in the app
        {
            var books = sp.GetRequiredService<IBookService>();
            Assert.True((await books.DeleteAsync(id)).Succeeded);
            Assert.Null(await books.GetDetailsAsync(id));
        });
    }
}
