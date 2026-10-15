using CseLibrary.Core.Interfaces;

namespace CseLibrary.Core.Entities;

/// <summary>A title or edition; its physical copies are <see cref="BookCopy"/> rows (FR-M15-01).</summary>
public class Book : BaseEntity, IAuditable
{
    public string? Isbn { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Authors { get; set; } // ponytail: free text; Authors/BookAuthors tables if author search or reports need them
    public string? Publisher { get; set; }
    public string? Edition { get; set; }
    public int? PublicationYear { get; set; }
    public string? Category { get; set; } // ponytail: free text; a Categories table when filtering by category is built
    public string? Description { get; set; }
    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
}

public class BookCopy : BaseEntity, IAuditable
{
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public string AccessionNumber { get; set; } = string.Empty;
    public string? ShelfLocation { get; set; }
    public DateOnly? AcquisitionDate { get; set; }
    public CopyStatus CopyStatus { get; set; } = CopyStatus.Available;
    public string? Notes { get; set; }
}

/// <summary>Archive FR-03. <see cref="Issued"/> is set only by lending (Phase 3), never by catalog edits.</summary>
public enum CopyStatus : byte
{
    Available = 1,
    Issued = 2,
    Lost = 3,
    Damaged = 4,
    Maintenance = 5,
    Retired = 6
}
