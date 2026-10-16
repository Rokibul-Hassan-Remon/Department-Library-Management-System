using System.Linq.Expressions;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Core.Paging;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

public class LibraryMemberRepository(ApplicationDbContext db) : Repository<LibraryMember>(db), ILibraryMemberRepository
{
    public async Task<PagedResult<LibraryMemberDto>> GetPageAsync(PageRequest page, string? search, MemberType? type, CancellationToken ct = default)
    {
        var members = Db.LibraryMembers.Where(m => type == null || m.MemberType == type);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            members = members.Where(m => m.MemberCode == s || m.FullName.Contains(s) || m.Email == s);
        }
        var total = await members.CountAsync(ct);
        page = page.ClampTo(total);
        var items = await members.OrderBy(m => m.FullName).ThenBy(m => m.Id).Skip(page.Skip).Take(page.PageSize)
            .Select(m => new LibraryMemberDto
            {
                Id = m.Id, MemberCode = m.MemberCode, FullName = m.FullName, MemberType = m.MemberType, Email = m.Email,
                Phone = m.Phone, IsActive = m.Status == RecordStatus.Active,
                ActiveLoans = m.Loans.Count(l => l.TransactionStatus == LendingStatus.Issued)
            }).ToListAsync(ct);
        return new PagedResult<LibraryMemberDto>(items, total, page.Page, page.PageSize);
    }

    public Task<LibraryMemberDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default) =>
        Db.LibraryMembers.Where(m => m.Id == id).Select(m => new LibraryMemberDetailsDto
        {
            Id = m.Id, MemberCode = m.MemberCode, FullName = m.FullName, MemberType = m.MemberType, Email = m.Email,
            Phone = m.Phone, IsActive = m.Status == RecordStatus.Active,
            ActiveLoans = m.Loans.Count(l => l.TransactionStatus == LendingStatus.Issued),
            Loans = m.Loans.OrderByDescending(l => l.IssueDateUtc).Take(100).AsQueryable().Select(LoanRepository.ToDto).ToList()
        }).FirstOrDefaultAsync(ct);

    public Task<int?> FindIdByCodeAsync(string memberCode, CancellationToken ct = default) =>
        Db.LibraryMembers.Where(m => m.MemberCode == memberCode).Select(m => (int?)m.Id).FirstOrDefaultAsync(ct);

    public Task<bool> CodeExistsAsync(string memberCode, int? excludeId, CancellationToken ct = default) =>
        Db.LibraryMembers.AnyAsync(m => m.MemberCode == memberCode && m.Id != excludeId, ct);

    public Task<int?> FindIdByUserIdAsync(string userId, CancellationToken ct = default) =>
        Db.LibraryMembers.Where(m => m.IdentityUserId == userId).Select(m => (int?)m.Id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<PendingRegistrationDto>> GetPendingAsync(CancellationToken ct = default) =>
        await Db.LibraryMembers.Where(m => m.Approval == ApprovalStatus.Pending).OrderBy(m => m.CreatedAt)
            .Select(m => new PendingRegistrationDto { LibraryMemberId = m.Id, MemberCode = m.MemberCode, FullName = m.FullName, Email = m.Email, CreatedAt = m.CreatedAt })
            .ToListAsync(ct);
}

public class LoanRepository(ApplicationDbContext db) : Repository<Loan>(db), ILoanRepository
{
    internal static readonly Expression<Func<Loan, LoanDto>> ToDto = l => new LoanDto
    {
        Id = l.Id, LibraryMemberId = l.LibraryMemberId, MemberCode = l.LibraryMember.MemberCode, MemberName = l.LibraryMember.FullName,
        BookId = l.BookCopy.BookId, BookTitle = l.BookCopy.Book.Title, AccessionNumber = l.BookCopy.AccessionNumber,
        IssueDateUtc = l.IssueDateUtc, DueDate = l.DueDate, ReturnDateUtc = l.ReturnDateUtc, RenewCount = l.RenewCount
    };

    public async Task<PagedResult<LoanDto>> GetPageAsync(PageRequest page, LoanFilter filter, string? search, DateOnly today,
        DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default)
    {
        var loans = filter switch
        {
            LoanFilter.Active => Db.Loans.Where(l => l.TransactionStatus == LendingStatus.Issued),
            LoanFilter.Overdue => Db.Loans.Where(l => l.TransactionStatus == LendingStatus.Issued && l.DueDate < today),
            _ => Db.Loans
        };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            loans = loans.Where(l => l.LibraryMember.MemberCode == s || l.LibraryMember.FullName.Contains(s)
                                     || l.BookCopy.AccessionNumber == s || l.BookCopy.Book.Title.Contains(s));
        }
        if (fromUtc != null && toUtc != null)
            loans = loans.Where(l => (l.IssueDateUtc >= fromUtc && l.IssueDateUtc < toUtc) || (l.ReturnDateUtc >= fromUtc && l.ReturnDateUtc < toUtc));
        var total = await loans.CountAsync(ct);
        page = page.ClampTo(total);
        var items = await (filter == LoanFilter.All ? loans.OrderByDescending(l => l.IssueDateUtc) : loans.OrderBy(l => l.DueDate))
            .ThenBy(l => l.Id).Skip(page.Skip).Take(page.PageSize).Select(ToDto).ToListAsync(ct);
        return new PagedResult<LoanDto>(items, total, page.Page, page.PageSize);
    }

    public Task<int> CountActiveAsync(int memberId, CancellationToken ct = default) =>
        Db.Loans.CountAsync(l => l.LibraryMemberId == memberId && l.TransactionStatus == LendingStatus.Issued, ct);

    public Task<bool> HasOverdueAsync(int memberId, DateOnly today, CancellationToken ct = default) =>
        Db.Loans.AnyAsync(l => l.LibraryMemberId == memberId && l.TransactionStatus == LendingStatus.Issued && l.DueDate < today, ct);

    public Task<bool> HasActiveAsync(int memberId, CancellationToken ct = default) =>
        Db.Loans.AnyAsync(l => l.LibraryMemberId == memberId && l.TransactionStatus == LendingStatus.Issued, ct);

    public async Task<IReadOnlyList<ReminderTarget>> GetReminderTargetsAsync(DateOnly dueOn, CancellationToken ct = default) =>
        await Db.Loans.Where(l => l.TransactionStatus == LendingStatus.Issued && l.DueDate <= dueOn && l.LibraryMember.Email != null)
            .OrderBy(l => l.Id)
            .Select(l => new ReminderTarget(l.Id, l.LibraryMember.Email!, l.LibraryMember.FullName, l.BookCopy.Book.Title, l.BookCopy.AccessionNumber, l.DueDate))
            .ToListAsync(ct);

    public Task<Loan?> GetActiveForUpdateAsync(int id, CancellationToken ct = default) =>
        Db.Loans.Include(l => l.BookCopy).Include(l => l.LibraryMember).FirstOrDefaultAsync(l => l.Id == id && l.TransactionStatus == LendingStatus.Issued, ct);
}
