using System.ComponentModel.DataAnnotations;
using CseLibrary.Core.Entities;

namespace CseLibrary.Core.DTOs;

/// <summary>Create (Id = 0) and edit form; also the list row.</summary>
public class LibraryMemberDto
{
    public int Id { get; set; }
    [Required, StringLength(30), Display(Name = "Member code")] public string MemberCode { get; set; } = string.Empty;
    [Required, StringLength(150), Display(Name = "Full name")] public string FullName { get; set; } = string.Empty;
    [Display(Name = "Type")] public MemberType MemberType { get; set; } = MemberType.Student;
    [EmailAddress, StringLength(256)] public string? Email { get; set; }
    [StringLength(30)] public string? Phone { get; set; }
    [Display(Name = "Active")] public bool IsActive { get; set; } = true;
    public int ActiveLoans { get; set; }
}

public class LibraryMemberDetailsDto : LibraryMemberDto
{
    public IReadOnlyList<LoanDto> Loans { get; set; } = [];
}

public class LoanDto
{
    public int Id { get; set; }
    public int LibraryMemberId { get; set; }
    public string MemberCode { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string AccessionNumber { get; set; } = string.Empty;
    public DateTime IssueDateUtc { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime? ReturnDateUtc { get; set; }
    public int RenewCount { get; set; }
    public bool IsOverdue(DateOnly today) => ReturnDateUtc == null && DueDate < today;
}

public class IssueDto
{
    [Required, StringLength(30), Display(Name = "Member code")] public string MemberCode { get; set; } = string.Empty;
    [Required, StringLength(50), Display(Name = "Accession no.")] public string AccessionNumber { get; set; } = string.Empty;
}

public enum LoanFilter { Active, Overdue, All }

public class RegisterDto
{
    [Required, StringLength(30), Display(Name = "Student / staff ID")] public string MemberCode { get; set; } = string.Empty;
    [Required, StringLength(150), Display(Name = "Full name")] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), StringLength(100)] public string Password { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    [Display(Name = "I am a")] public RegistrationType RegistrationType { get; set; } = RegistrationType.Student;
}

public class PendingRegistrationDto
{
    public int LibraryMemberId { get; set; }
    public string MemberCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>One reminder email: a loan due soon or overdue.</summary>
public record ReminderTarget(int LoanId, string Email, string MemberName, string BookTitle, string AccessionNumber, DateOnly DueDate);
