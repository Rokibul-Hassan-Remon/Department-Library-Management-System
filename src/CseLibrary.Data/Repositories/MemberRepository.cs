using System.Linq.Expressions;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Core.Paging;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

public class MemberRepository(ApplicationDbContext db) : Repository<Member>(db), IMemberRepository
{
    public async Task<PagedResult<MemberDto>> GetPageAsync(PageRequest page, string? search, CancellationToken ct = default)
    {
        var members = Db.Members.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            members = members.Where(m => m.MemberCode == s || m.FullName.Contains(s) || m.Email == s);
        }
        var total = await members.CountAsync(ct);
        page = page.ClampTo(total);
        var items = await members.OrderBy(m => m.FullName).ThenBy(m => m.Id).Skip(page.Skip).Take(page.PageSize)
            .Select(m => new MemberDto
            {
                Id = m.Id, MemberCode = m.MemberCode, FullName = m.FullName, MemberType = m.MemberType, Email = m.Email,
                Phone = m.Phone, IsActive = m.Status == RecordStatus.Active,
                ActiveLoans = m.Loans.Count(l => l.ReturnedAtUtc == null)
            }).ToListAsync(ct);
        return new PagedResult<MemberDto>(items, total, page.Page, page.PageSize);
    }

    public Task<MemberDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default) =>
        Db.Members.Where(m => m.Id == id).Select(m => new MemberDetailsDto
        {
            Id = m.Id, MemberCode = m.MemberCode, FullName = m.FullName, MemberType = m.MemberType, Email = m.Email,
            Phone = m.Phone, IsActive = m.Status == RecordStatus.Active,
            ActiveLoans = m.Loans.Count(l => l.ReturnedAtUtc == null),
            Loans = m.Loans.OrderByDescending(l => l.IssuedAtUtc).Take(100).AsQueryable().Select(LoanRepository.ToDto).ToList()
        }).FirstOrDefaultAsync(ct);

    public Task<int?> FindIdByCodeAsync(string memberCode, CancellationToken ct = default) =>
        Db.Members.Where(m => m.MemberCode == memberCode).Select(m => (int?)m.Id).FirstOrDefaultAsync(ct);

    public Task<bool> CodeExistsAsync(string memberCode, int? excludeId, CancellationToken ct = default) =>
        Db.Members.AnyAsync(m => m.MemberCode == memberCode && m.Id != excludeId, ct);

    public Task<int?> FindIdByUserIdAsync(string userId, CancellationToken ct = default) =>
        Db.Members.Where(m => m.UserId == userId).Select(m => (int?)m.Id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<PendingRegistrationDto>> GetPendingAsync(CancellationToken ct = default) =>
        await Db.Members.Where(m => m.Approval == ApprovalStatus.Pending).OrderBy(m => m.CreatedAt)
            .Select(m => new PendingRegistrationDto { MemberId = m.Id, MemberCode = m.MemberCode, FullName = m.FullName, Email = m.Email, CreatedAt = m.CreatedAt })
            .ToListAsync(ct);
}

public class LoanRepository(ApplicationDbContext db) : Repository<Loan>(db), ILoanRepository
{
    internal static readonly Expression<Func<Loan, LoanDto>> ToDto = l => new LoanDto
    {
        Id = l.Id, MemberId = l.MemberId, MemberCode = l.Member.MemberCode, MemberName = l.Member.FullName,
        BookId = l.BookCopy.BookId, BookTitle = l.BookCopy.Book.Title, AccessionNumber = l.BookCopy.AccessionNumber,
        IssuedAtUtc = l.IssuedAtUtc, DueDate = l.DueDate, ReturnedAtUtc = l.ReturnedAtUtc, RenewCount = l.RenewCount
    };

    public async Task<PagedResult<LoanDto>> GetPageAsync(PageRequest page, LoanFilter filter, string? search, DateOnly today, CancellationToken ct = default)
    {
        var loans = filter switch
        {
            LoanFilter.Active => Db.Loans.Where(l => l.ReturnedAtUtc == null),
            LoanFilter.Overdue => Db.Loans.Where(l => l.ReturnedAtUtc == null && l.DueDate < today),
            _ => Db.Loans
        };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            loans = loans.Where(l => l.Member.MemberCode == s || l.Member.FullName.Contains(s)
                                     || l.BookCopy.AccessionNumber == s || l.BookCopy.Book.Title.Contains(s));
        }
        var total = await loans.CountAsync(ct);
        page = page.ClampTo(total);
        var items = await (filter == LoanFilter.All ? loans.OrderByDescending(l => l.IssuedAtUtc) : loans.OrderBy(l => l.DueDate))
            .ThenBy(l => l.Id).Skip(page.Skip).Take(page.PageSize).Select(ToDto).ToListAsync(ct);
        return new PagedResult<LoanDto>(items, total, page.Page, page.PageSize);
    }

    public Task<int> CountActiveAsync(int memberId, CancellationToken ct = default) =>
        Db.Loans.CountAsync(l => l.MemberId == memberId && l.ReturnedAtUtc == null, ct);

    public Task<bool> HasOverdueAsync(int memberId, DateOnly today, CancellationToken ct = default) =>
        Db.Loans.AnyAsync(l => l.MemberId == memberId && l.ReturnedAtUtc == null && l.DueDate < today, ct);

    public Task<bool> HasActiveAsync(int memberId, CancellationToken ct = default) =>
        Db.Loans.AnyAsync(l => l.MemberId == memberId && l.ReturnedAtUtc == null, ct);

    public async Task<IReadOnlyList<ReminderTarget>> GetReminderTargetsAsync(DateOnly dueOn, DateOnly today, CancellationToken ct = default) =>
        await Db.Loans.Where(l => l.ReturnedAtUtc == null && (l.DueDate == dueOn || l.DueDate < today) && l.Member.Email != null)
            .OrderBy(l => l.Id)
            .Select(l => new ReminderTarget(l.Member.Email!, l.Member.FullName, l.BookCopy.Book.Title, l.BookCopy.AccessionNumber, l.DueDate))
            .ToListAsync(ct);

    public Task<Loan?> GetActiveForUpdateAsync(int id, CancellationToken ct = default) =>
        Db.Loans.Include(l => l.BookCopy).Include(l => l.Member).FirstOrDefaultAsync(l => l.Id == id && l.ReturnedAtUtc == null, ct);
}
