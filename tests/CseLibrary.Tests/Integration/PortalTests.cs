using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;
using CseLibrary.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

/// <summary>Self-registration (D-54), the member's own loans, and due-date reminders.</summary>
[Collection(AppCollection.Name)]
public class PortalTests(AppFactory app)
{
    private static string Unique(string prefix) => prefix + Guid.NewGuid().ToString("N")[..10];

    private static RegisterDto Form(RegistrationType type, string? code = null) => new()
    {
        MemberCode = code ?? Unique("R"), FullName = "Applicant", Email = Unique("r") + "@test.local",
        Password = "Passw0rd!12", ConfirmPassword = "Passw0rd!12", RegistrationType = type
    };

    private Task<Result> Register(RegisterDto dto) =>
        app.WithServiceAsync(sp => sp.GetRequiredService<IRegistrationService>().RegisterAsync(dto));

    private Task<bool> CanSignIn(string email) => app.WithServiceAsync(async sp =>
    {
        var user = (await sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email))!;
        return await sp.GetRequiredService<SignInManager<ApplicationUser>>().CanSignInAsync(user);
    });

    private Task<LibraryMemberDetailsDto?> MemberOf(string email) => app.WithServiceAsync(async sp =>
    {
        var user = (await sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email))!;
        return await sp.GetRequiredService<ILibraryMemberService>().GetForUserAsync(user.Id);
    });

    [Fact]
    public async Task Student_is_active_at_once_and_gets_no_role()
    {
        var form = Form(RegistrationType.Student);
        Assert.True((await Register(form)).Succeeded);

        Assert.True(await CanSignIn(form.Email));
        var member = (await MemberOf(form.Email))!;
        Assert.Equal((MemberType.Student, true), (member.MemberType, member.IsActive));
        var roles = await app.WithServiceAsync(async sp =>
        {
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            return await users.GetRolesAsync((await users.FindByEmailAsync(form.Email))!);
        });
        Assert.Empty(roles); // D-54 item 6
    }

    [Fact]
    public async Task Fellow_waits_for_review_then_signs_in_after_approval()
    {
        var form = Form(RegistrationType.DepartmentFellow);
        Assert.True((await Register(form)).Succeeded);
        Assert.False(await CanSignIn(form.Email));

        var memberId = (await MemberOf(form.Email))!.Id;
        await app.WithServiceAsync(async sp =>
        {
            var registrations = sp.GetRequiredService<IRegistrationService>();
            Assert.Contains(await registrations.GetPendingAsync(), p => p.LibraryMemberId == memberId);
            Assert.True((await registrations.ApproveAsync(memberId)).Succeeded);
        });
        Assert.True(await CanSignIn(form.Email));
        Assert.Equal(MemberType.Fellow, (await MemberOf(form.Email))!.MemberType);
        Assert.Equal(ErrorCodes.NotFound,
            (await app.WithServiceAsync(sp => sp.GetRequiredService<IRegistrationService>().RejectAsync(memberId))).ErrorCode);
    }

    [Fact]
    public async Task Rejected_fellow_cannot_sign_in()
    {
        var form = Form(RegistrationType.DepartmentFellow);
        Assert.True((await Register(form)).Succeeded);
        var memberId = (await MemberOf(form.Email))!.Id;
        Assert.True((await app.WithServiceAsync(sp => sp.GetRequiredService<IRegistrationService>().RejectAsync(memberId))).Succeeded);
        Assert.False(await CanSignIn(form.Email));
    }

    [Fact]
    public async Task Undefined_type_is_refused_and_creates_nothing()
    {
        var form = Form((RegistrationType)7);
        Assert.Equal(ErrorCodes.InvalidState, (await Register(form)).ErrorCode);
        Assert.Null(await app.WithServiceAsync(sp => sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(form.Email)));
    }

    [Fact]
    public async Task Existing_member_is_linked_not_duplicated_and_cannot_register_twice()
    {
        var code = Unique("R");
        var memberId = (await app.WithServiceAsync(sp => sp.GetRequiredService<ILibraryMemberService>()
            .CreateAsync(new LibraryMemberDto { MemberCode = code, FullName = "Created by staff" }))).Value;

        var form = Form(RegistrationType.Student, code);
        Assert.True((await Register(form)).Succeeded);
        Assert.Equal(memberId, (await MemberOf(form.Email))!.Id);
        Assert.Equal(ErrorCodes.AlreadyRegistered, (await Register(Form(RegistrationType.Student, code))).ErrorCode);
    }

    [Fact]
    public async Task Reminders_cover_loans_due_soon_and_overdue_only()
    {
        var code = Unique("M");
        var (soon, later) = (Unique("ACC"), Unique("ACC"));
        await app.WithServiceAsync(async sp =>
        {
            var books = sp.GetRequiredService<IBookService>();
            var bookId = (await books.CreateAsync(new BookFormDto { Title = Unique("Reminder ") })).Value;
            foreach (var a in new[] { soon, later }) await books.AddCopyAsync(new BookCopyDto { BookId = bookId, AccessionNumber = a });
            await sp.GetRequiredService<ILibraryMemberService>().CreateAsync(new LibraryMemberDto { MemberCode = code, FullName = "R", Email = code + "@test.local", MemberType = MemberType.Fellow });
            var lending = sp.GetRequiredService<ILendingService>();
            Assert.True((await lending.IssueAsync(new IssueDto { MemberCode = code, AccessionNumber = soon })).Succeeded);
            Assert.True((await lending.IssueAsync(new IssueDto { MemberCode = code, AccessionNumber = later })).Succeeded);
        });
        var today = DateOnly.FromDateTime(DateTime.Now);
        await app.WithServiceAsync(async sp => // the "soon" loan becomes due in 2 days (the default reminder lead)
        {
            var uow = sp.GetRequiredService<IUnitOfWork>();
            var page = await sp.GetRequiredService<ILendingService>().GetPageAsync(new(), LoanFilter.Active, soon);
            var loan = (await uow.Loans.GetForUpdateAsync(page.Items.Single().Id))!;
            loan.DueDate = today.AddDays(2);
            await uow.SaveChangesAsync();
        });

        var targets = await app.WithServiceAsync(sp => sp.GetRequiredService<IUnitOfWork>().Loans.GetReminderTargetsAsync(today.AddDays(2), today));
        Assert.Contains(targets, t => t.AccessionNumber == soon);
        Assert.DoesNotContain(targets, t => t.AccessionNumber == later); // due in 30 days
        Assert.True(await app.WithServiceAsync(sp => sp.GetRequiredService<IReminderService>().SendDueRemindersAsync()) >= 1);
        Assert.Equal(0, await app.WithServiceAsync(sp => sp.GetRequiredService<IReminderService>().SendDueRemindersAsync())); // idempotency key: never twice
    }
}
