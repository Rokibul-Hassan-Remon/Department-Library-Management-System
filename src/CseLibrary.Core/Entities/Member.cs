using CseLibrary.Core.Interfaces;

namespace CseLibrary.Core.Entities;

/// <summary>A borrower. Status Inactive (BaseEntity) blocks new loans (BR-03).</summary>
public class Member : BaseEntity, IAuditable
{
    public string MemberCode { get; set; } = string.Empty; // student ID or staff ID
    public string FullName { get; set; } = string.Empty;
    public MemberType MemberType { get; set; } = MemberType.Student;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}

public enum MemberType : byte
{
    Student = 1,
    Fellow = 2
}

/// <summary>One issue of one copy; never overwritten, so it is the lending history (FR-M15-03).</summary>
public class Loan : BaseEntity, IAuditable
{
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public int BookCopyId { get; set; }
    public BookCopy BookCopy { get; set; } = null!;
    public DateTime IssuedAtUtc { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime? ReturnedAtUtc { get; set; }
    public int RenewCount { get; set; }
}
