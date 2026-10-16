using System.Net;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

/// <summary>FR-12 reports: filters reach the database query, and every page renders.</summary>
[Collection(AppCollection.Name)]
public class ReportTests(AppFactory app)
{
    private static string Unique(string prefix) => prefix + Guid.NewGuid().ToString("N")[..10];

    [Fact]
    public async Task Report_filters_select_the_right_rows()
    {
        var (code, issued, lost) = (Unique("M"), Unique("ACC"), Unique("ACC"));
        await app.WithServiceAsync(async sp =>
        {
            var books = sp.GetRequiredService<IBookService>();
            var bookId = (await books.CreateAsync(new BookFormDto { Title = Unique("Report ") })).Value;
            await books.AddCopyAsync(new BookCopyDto { BookId = bookId, AccessionNumber = issued });
            await books.AddCopyAsync(new BookCopyDto { BookId = bookId, AccessionNumber = lost, CopyStatus = CopyStatus.Lost });
            await sp.GetRequiredService<ILibraryMemberService>().CreateAsync(new LibraryMemberDto { MemberCode = code, FullName = "Report Fellow", MemberType = MemberType.Fellow });
            Assert.True((await sp.GetRequiredService<ILendingService>().IssueAsync(new IssueDto { MemberCode = code, AccessionNumber = issued })).Succeeded);
        });

        var today = DateOnly.FromDateTime(DateTime.Now);
        var all = new PageRequest(1, PageRequest.MaxPageSize);
        await app.WithServiceAsync(async sp =>
        {
            var reports = sp.GetRequiredService<IReportService>();
            Assert.Contains((await reports.GetLoansAsync(all, LoanFilter.All, code, today, today)).Items, l => l.AccessionNumber == issued);
            Assert.Empty((await reports.GetLoansAsync(all, LoanFilter.All, code, today.AddDays(-10), today.AddDays(-1))).Items);
            Assert.Contains((await reports.GetLoansAsync(all, LoanFilter.All, code, null, today)).Items, l => l.AccessionNumber == issued); // open start

            var lostCopies = await reports.GetCopiesAsync(all, CopyStatus.Lost);
            Assert.Contains(lostCopies.Items, c => c.AccessionNumber == lost);
            Assert.All(lostCopies.Items, c => Assert.Equal(CopyStatus.Lost, c.CopyStatus));
            Assert.True((await reports.CountCopiesAsync())[CopyStatus.Lost] >= 1);

            Assert.All((await reports.GetMembersAsync(all, MemberType.Fellow)).Items, m => Assert.Equal(MemberType.Fellow, m.MemberType));
        });
    }

    [Theory]
    [InlineData("/Reports")]
    [InlineData("/Reports/Copies?status=Available")]
    [InlineData("/Reports/Loans?filter=Overdue&from=2026-01-01&to=2026-12-31")]
    [InlineData("/Reports/Members?type=Student")]
    [InlineData("/Reports/Emails?status=Failed")]
    public async Task Report_pages_render_for_a_user_with_the_permission(string url)
    {
        var client = await app.LoggedInClient(AppFactory.AdminEmail, AppFactory.AdminPassword);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).StatusCode);
    }
}
