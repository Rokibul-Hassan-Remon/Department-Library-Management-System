using CseLibrary.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CseLibrary.Data.Data.Configurations;

// Database model M15 (docs/diagrams/database-model.html) and archive §4.2. FKs are NO ACTION: no delete while history exists.

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books", t => t.HasCheckConstraint("CK_Books_PublicationYear", "[PublicationYear] IS NULL OR [PublicationYear] BETWEEN 1000 AND 9999"));
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).UseHiLo("Books_HiLo"); // D-29
        builder.Property(b => b.Isbn).HasMaxLength(20).Audited();
        builder.Property(b => b.Title).IsRequired().HasMaxLength(250).Audited();
        builder.Property(b => b.Publisher).HasMaxLength(200).Audited();
        builder.Property(b => b.Edition).HasMaxLength(50).Audited();
        builder.Property(b => b.PublicationYear).Audited();
        builder.Property(b => b.CategoryId).Audited();
        builder.Property(b => b.Description).HasMaxLength(2000).Audited();
        builder.Property(b => b.CoverImagePath).HasMaxLength(500).Audited();
        builder.ClassifyBaseColumns();
        builder.HasIndex(b => b.Isbn).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter + " AND [Isbn] IS NOT NULL");
        builder.HasIndex(b => b.Title);
        builder.HasOne(b => b.Category).WithMany().HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseHiLo("Categories_HiLo");
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100).Audited();
        builder.ClassifyBaseColumns();
        builder.HasIndex(c => c.Name).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter);
    }
}

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseHiLo("Authors_HiLo");
        builder.Property(a => a.Name).IsRequired().HasMaxLength(150).Audited();
        builder.Property(a => a.PhotoPath).HasMaxLength(500).Audited();
        builder.ClassifyBaseColumns();
        // Unique so find-or-create by name cannot make two "Knuth" rows under concurrency.
        builder.HasIndex(a => a.Name).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter);
    }
}

public class BookAuthorConfiguration : IEntityTypeConfiguration<BookAuthor>
{
    public void Configure(EntityTypeBuilder<BookAuthor> builder)
    {
        builder.ToTable("BookAuthors");
        builder.HasKey(ba => new { ba.BookId, ba.AuthorId });
        builder.HasOne(ba => ba.Book).WithMany(b => b.BookAuthors).HasForeignKey(ba => ba.BookId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(ba => ba.Author).WithMany().HasForeignKey(ba => ba.AuthorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(ba => ba.AuthorId);
    }
}

public class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
{
    public void Configure(EntityTypeBuilder<BookCopy> builder)
    {
        builder.ToTable("BookCopies", t => t.HasCheckConstraint("CK_BookCopies_CopyStatus", "[CopyStatus] BETWEEN 1 AND 6"));
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseHiLo("BookCopies_HiLo"); // D-29
        builder.Property(c => c.BookId).Audited();
        builder.Property(c => c.AccessionNumber).IsRequired().HasMaxLength(50).Audited();
        builder.Property(c => c.ShelfLocation).HasMaxLength(100).Audited();
        builder.Property(c => c.AcquisitionDate).Audited();
        builder.Property(c => c.CopyStatus).Audited();
        builder.Property(c => c.Notes).HasMaxLength(500).Audited();
        builder.Property(c => c.RowVersion).IsRowVersion().NotAudited(); // a stale copy edit or issue fails instead of overwriting
        builder.ClassifyBaseColumns();
        builder.HasIndex(c => c.AccessionNumber).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter);
        builder.HasOne(c => c.Book).WithMany(b => b.Copies).HasForeignKey(c => c.BookId).OnDelete(DeleteBehavior.Restrict);
    }
}
