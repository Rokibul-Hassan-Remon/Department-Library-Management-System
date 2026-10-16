using CseLibrary.Core.Interfaces;

namespace CseLibrary.Core.Entities;

/// <summary>A borrower. Status Inactive (BaseEntity) blocks new loans (BR-03).</summary>
public class LibraryMember : BaseEntity, IAuditable
{
    public string MemberCode { get; set; } = string.Empty; // student ID or staff ID
    public string FullName { get; set; } = string.Empty;
    public MemberType MemberType { get; set; } = MemberType.Student;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? IdentityUserId { get; set; } // the portal login (0..1), set by self-registration (D-54 Q-S19-5)
    public ApprovalStatus Approval { get; set; } = ApprovalStatus.Approved;
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}

/// <summary>Registration review state (D-54 Q-S19-8), separate from the account Status.</summary>
public enum ApprovalStatus : byte
{
    Approved = 1,
    Pending = 2,
    Rejected = 3
}

/// <summary>The applicant's choice on the public form (D-54 Session 21). Never a role or permission.</summary>
public enum RegistrationType : byte
{
    Student = 1,
    DepartmentFellow = 2
}

public enum MemberType : byte
{
    Student = 1,
    Fellow = 2
}

/// <summary>
/// One issue of one copy, stored in <c>LendingTransactions</c>; never deleted, so it is the lending history
/// (FR-M15-03). Overdue is derived (Issued and DueDate before today), never stored.
/// </summary>
public class Loan : BaseEntity, IAuditable
{
    public int LibraryMemberId { get; set; }
    public LibraryMember LibraryMember { get; set; } = null!;
    public int BookCopyId { get; set; }
    public BookCopy BookCopy { get; set; } = null!;
    public DateTime IssueDateUtc { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime? ReturnDateUtc { get; set; }
    public LendingStatus TransactionStatus { get; set; } = LendingStatus.Issued;
    public string IssuedByUserId { get; set; } = string.Empty;
    public string? ReturnedByUserId { get; set; }
    public ReturnCondition? ReturnCondition { get; set; }
    public int RenewCount { get; set; }
}

public enum LendingStatus : byte
{
    Issued = 1,
    Returned = 2
}

public enum ReturnCondition : byte
{
    Good = 1,
    Damaged = 2
}

/// <summary>Borrowing rules per member type (D-48 values chosen Session 23).</summary>
public class LibraryPolicy : BaseEntity, IAuditable
{
    public MemberType MemberCategory { get; set; }
    public int MaxActiveLoans { get; set; }
    public int LoanDays { get; set; }
    public int MaxRenewals { get; set; }
}

/// <summary>One reminder email per <see cref="IdempotencyKey"/>; a failed send is retried on the next run.</summary>
public class EmailNotificationLog : BaseEntity
{
    public int LendingTransactionId { get; set; }
    public Loan LendingTransaction { get; set; } = null!;
    public EmailType EmailType { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public string? LastError { get; set; }
}

public enum EmailType : byte
{
    DueSoon = 1,
    Overdue = 2
}

public enum DeliveryStatus : byte
{
    Sent = 1,
    Failed = 2
}
