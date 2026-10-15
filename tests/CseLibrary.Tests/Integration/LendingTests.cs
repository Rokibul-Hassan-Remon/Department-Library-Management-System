using CseLibrary.Core.Constants;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;
using CseLibrary.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

/// <summary>Phase 1 lending: members, issue, return, renew (FR-M15-02/03, BR-03/04/16; policy defaults Session 23).</summary>
[Collection(AppCollection.Name)]
public class LendingTests(AppFactory app)
{
    private static string Unique(string prefix) => prefix + Guid.NewGuid().ToString("N")[..10];

    /// <summary>A member and <paramref name="copies"/> available copies of one new title.</summary>
    private async Task<(string Member, int MemberId, string[] Copies)> Arrange(MemberType type, int copies, bool active = true)
    {
        var code = Unique("M");
        var accessions = Enumerable.Range(0, copies).Select(_ => Unique("ACC")).ToArray();
        var memberId = await app.WithServiceAsync(async sp =>
        {
            var books = sp.GetRequiredService<IBookService>();
            var bookId = (await books.CreateAsync(new BookFormDto { Title = Unique("Lending ") })).Value;
            foreach (var a in accessions) Assert.True((await books.AddCopyAsync(new BookCopyDto { BookId = bookId, AccessionNumber = a })).Succeeded);
            return (await sp.GetRequiredService<IMemberService>().CreateAsync(
                new MemberDto { MemberCode = code, FullName = "Test Member", MemberType = type, IsActive = active })).Value;
        });
        return (code, memberId, accessions);
    }

    private Task<Result> Issue(string member, string accession) =>
        app.WithServiceAsync(sp => sp.GetRequiredService<ILendingService>().IssueAsync(new IssueDto { MemberCode = member, AccessionNumber = accession }));

    private Task<MemberDetailsDto> Member(int id) =>
        app.WithServiceAsync(async sp => (await sp.GetRequiredService<IMemberService>().GetDetailsAsync(id))!);

    [Fact]
    public async Task Issue_and_return_keep_history_and_copy_status_in_step()
    {
        var (member, memberId, copies) = await Arrange(MemberType.Student, 1);
        Assert.True((await Issue(member, copies[0])).Succeeded);

        var loan = Assert.Single((await Member(memberId)).Loans);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now).AddDays(14), loan.DueDate); // Student default
        var other = await Arrange(MemberType.Student, 0);
        Assert.Equal(ErrorCodes.CopyUnavailable, (await Issue(other.Member, copies[0])).ErrorCode); // BR-04

        Assert.True((await app.WithServiceAsync(sp => sp.GetRequiredService<ILendingService>().ReturnAsync(loan.Id))).Succeeded);
        var returned = Assert.Single((await Member(memberId)).Loans);
        Assert.NotNull(returned.ReturnDateUtc); // history row kept, not deleted
        Assert.True((await Issue(other.Member, copies[0])).Succeeded); // available again
    }

    [Fact]
    public async Task Limit_inactive_member_and_overdue_block_new_issues()
    {
        var (member, memberId, copies) = await Arrange(MemberType.Student, 4);
        for (var i = 0; i < 3; i++) Assert.True((await Issue(member, copies[i])).Succeeded);
        Assert.Equal(ErrorCodes.LimitReached, (await Issue(member, copies[3])).ErrorCode); // Student max 3

        var inactive = await Arrange(MemberType.Fellow, 1, active: false);
        Assert.Equal(ErrorCodes.MemberInactive, (await Issue(inactive.Member, inactive.Copies[0])).ErrorCode); // BR-03

        var late = await Arrange(MemberType.Fellow, 2);
        Assert.True((await Issue(late.Member, late.Copies[0])).Succeeded);
        var loanId = Assert.Single((await Member(late.MemberId)).Loans).Id;
        await MakeOverdue(loanId);
        Assert.Equal(ErrorCodes.HasOverdue, (await Issue(late.Member, late.Copies[1])).ErrorCode);
        Assert.Equal(ErrorCodes.RenewNotAllowed,
            (await app.WithServiceAsync(sp => sp.GetRequiredService<ILendingService>().RenewAsync(loanId))).ErrorCode);
    }

    [Fact]
    public async Task Damaged_return_records_who_and_how_and_retires_the_copy_from_lending()
    {
        var (member, memberId, copies) = await Arrange(MemberType.Student, 1);
        Assert.True((await Issue(member, copies[0])).Succeeded);
        var loanId = Assert.Single((await Member(memberId)).Loans).Id;
        Assert.True((await app.WithServiceAsync(sp => sp.GetRequiredService<ILendingService>().ReturnAsync(loanId, ReturnCondition.Damaged))).Succeeded);

        var other = await Arrange(MemberType.Student, 0);
        Assert.Equal(ErrorCodes.CopyUnavailable, (await Issue(other.Member, copies[0])).ErrorCode); // Damaged, not Available
        var loan = await app.WithServiceAsync(sp => sp.GetRequiredService<IUnitOfWork>().Loans.GetForUpdateAsync(loanId));
        Assert.Equal((LendingStatus.Returned, ReturnCondition.Damaged, TestCurrentUser.SeedAdminId, TestCurrentUser.SeedAdminId),
            (loan!.TransactionStatus, loan.ReturnCondition, loan.IssuedByUserId, loan.ReturnedByUserId));
    }

    [Fact]
    public async Task Renew_extends_once_then_is_refused()
    {
        var (member, memberId, copies) = await Arrange(MemberType.Fellow, 1);
        Assert.True((await Issue(member, copies[0])).Succeeded);
        var loanId = Assert.Single((await Member(memberId)).Loans).Id;

        var lending = (Func<Task<Result>>)(() => app.WithServiceAsync(sp => sp.GetRequiredService<ILendingService>().RenewAsync(loanId)));
        Assert.True((await lending()).Succeeded);
        Assert.Equal(1, Assert.Single((await Member(memberId)).Loans).RenewCount);
        Assert.Equal(ErrorCodes.RenewNotAllowed, (await lending()).ErrorCode); // default MaxRenewals 1
    }

    [Fact]
    public async Task Concurrent_issues_of_one_copy_commit_exactly_one_loan()
    {
        var (_, _, copies) = await Arrange(MemberType.Fellow, 1);
        var members = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => Arrange(MemberType.Fellow, 0)));

        var results = await Task.WhenAll(members.Select(m => Issue(m.Member, copies[0])));

        Assert.Single(results, r => r.Succeeded);
        Assert.All(results.Where(r => !r.Succeeded), r => Assert.Equal(ErrorCodes.CopyUnavailable, r.ErrorCode));
        var active = await app.WithServiceAsync(sp => sp.GetRequiredService<ILendingService>()
            .GetPageAsync(new PageRequest(), LoanFilter.Active, copies[0]));
        Assert.Equal(1, active.TotalCount);
    }

    [Fact]
    public async Task Concurrent_issues_for_one_member_never_exceed_the_limit()
    {
        var (member, memberId, copies) = await Arrange(MemberType.Student, 6);

        var results = await Task.WhenAll(copies.Select(c => Issue(member, c)));

        Assert.Equal(3, results.Count(r => r.Succeeded));
        Assert.All(results.Where(r => !r.Succeeded), r => Assert.Equal(ErrorCodes.LimitReached, r.ErrorCode));
        Assert.Equal(3, (await Member(memberId)).ActiveLoans);
    }

    [Fact]
    public async Task A_member_holding_books_is_not_deleted_and_lending_policy_is_seeded()
    {
        var (member, memberId, copies) = await Arrange(MemberType.Student, 1);
        Assert.True((await Issue(member, copies[0])).Succeeded);
        await app.WithServiceAsync(async sp =>
        {
            Assert.Equal(ErrorCodes.InUse, (await sp.GetRequiredService<IMemberService>().DeleteAsync(memberId)).ErrorCode);
            var configs = sp.GetRequiredService<IGeneralConfigService>();
            foreach (var (key, value) in LendingConfigKeys.Defaults) Assert.Equal(value, (await configs.GetByKeyAsync(key))?.Value);
        });
    }

    private Task MakeOverdue(int loanId) => app.WithServiceAsync(async sp =>
    {
        var uow = sp.GetRequiredService<IUnitOfWork>();
        var loan = (await uow.Loans.GetForUpdateAsync(loanId))!;
        loan.DueDate = DateOnly.FromDateTime(DateTime.Now).AddDays(-1);
        await uow.SaveChangesAsync();
    });
}
