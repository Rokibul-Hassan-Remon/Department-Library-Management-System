using System.ComponentModel.DataAnnotations;
using CseLibrary.Core.Entities;

namespace CseLibrary.Core.DTOs;

public class BookListItemDto
{
    public int Id { get; set; }
    public string? Isbn { get; set; }
    public string Title { get; set; } = string.Empty;
    public IReadOnlyList<string> AuthorNames { get; set; } = [];
    public string Authors => string.Join(", ", AuthorNames);
    public string? Category { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}

public class BookDetailsDto : BookFormDto
{
    public IReadOnlyList<BookCopyDto> Copies { get; set; } = [];
    public int AvailableCopies => Copies.Count(c => c.CopyStatus == CopyStatus.Available);
    public int IssuedCopies => Copies.Count(c => c.CopyStatus == CopyStatus.Issued);
    public int UnavailableCopies => Copies.Count - AvailableCopies - IssuedCopies;
}

/// <summary>Create (Id = 0) and edit form. Authors are typed comma separated; Category by name (found or created).</summary>
public class BookFormDto
{
    public int Id { get; set; }
    [StringLength(20)] public string? Isbn { get; set; }
    [Required, StringLength(250)] public string Title { get; set; } = string.Empty;
    [StringLength(1000)] public string? Authors { get; set; }
    [StringLength(200)] public string? Publisher { get; set; }
    [StringLength(50)] public string? Edition { get; set; }
    [Range(1000, 9999)] public short? PublicationYear { get; set; }
    [StringLength(100)] public string? Category { get; set; }
    [StringLength(2000)] public string? Description { get; set; }
}

public record CategoryDto(int Id, string Name);

/// <summary>Create (Id = 0) and edit form for one physical copy.</summary>
public class BookCopyDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string? BookTitle { get; set; }
    [Required, StringLength(50)] public string AccessionNumber { get; set; } = string.Empty;
    [StringLength(100)] public string? ShelfLocation { get; set; }
    public DateOnly? AcquisitionDate { get; set; }
    public CopyStatus CopyStatus { get; set; } = CopyStatus.Available;
    [StringLength(500)] public string? Notes { get; set; }
    public string? HeldBy { get; set; } // read-only: member holding an issued copy
    public byte[]? RowVersion { get; set; } // edit form round-trips it; a stale edit is refused
}

public class LibraryPolicyDto
{
    public int Id { get; set; }
    public MemberType MemberCategory { get; set; }
    [Range(0, 100), Display(Name = "Max books at once")] public int MaxActiveLoans { get; set; }
    [Range(1, 365), Display(Name = "Loan days")] public int LoanDays { get; set; }
    [Range(0, 10), Display(Name = "Max renewals")] public int MaxRenewals { get; set; }
}
