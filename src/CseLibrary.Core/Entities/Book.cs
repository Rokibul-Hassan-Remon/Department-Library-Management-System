using CseLibrary.Core.Interfaces;

namespace CseLibrary.Core.Entities;

/// <summary>A title or edition; its physical copies are <see cref="BookCopy"/> rows (FR-M15-01, database model M15).</summary>
public class Book : BaseEntity, IAuditable
{
    public string? Isbn { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Edition { get; set; }
    public short? PublicationYear { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? Description { get; set; }
    /// <summary>Relative to the web project, e.g. <c>Uploads/Books/{guid}.jpg</c> (D-59 diagram column).</summary>
    public string? CoverImagePath { get; set; }
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
}

public class Category : BaseEntity, IAuditable
{
    public string Name { get; set; } = string.Empty;
}

public class Author : BaseEntity, IAuditable
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Relative to the web project, e.g. <c>Uploads/Authors/{guid}.jpg</c>.</summary>
    public string? PhotoPath { get; set; }
}

/// <summary>Many-to-many join; <see cref="Order"/> keeps the authors in the order they were entered.</summary>
public class BookAuthor
{
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
    public byte Order { get; set; }
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
    public byte[] RowVersion { get; set; } = [];
}

/// <summary>Archive §4.2 values. <see cref="Issued"/> is set only by lending, never by catalog edits.</summary>
public enum CopyStatus : byte
{
    Available = 1,
    Issued = 2,
    Damaged = 3,
    Lost = 4,
    Maintenance = 5,
    Retired = 6
}
