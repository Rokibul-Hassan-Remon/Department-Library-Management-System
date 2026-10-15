using System.ComponentModel.DataAnnotations;
using CseLibrary.Core.Entities;

namespace CseLibrary.Core.DTOs;

public class BookListItemDto
{
    public int Id { get; set; }
    public string? Isbn { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Authors { get; set; }
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

/// <summary>Create (Id = 0) and edit form.</summary>
public class BookFormDto
{
    public int Id { get; set; }
    [StringLength(20)] public string? Isbn { get; set; }
    [Required, StringLength(300)] public string Title { get; set; } = string.Empty;
    [StringLength(300)] public string? Authors { get; set; }
    [StringLength(200)] public string? Publisher { get; set; }
    [StringLength(50)] public string? Edition { get; set; }
    [Range(1000, 9999)] public int? PublicationYear { get; set; }
    [StringLength(100)] public string? Category { get; set; }
    [StringLength(2000)] public string? Description { get; set; }
}

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
}
