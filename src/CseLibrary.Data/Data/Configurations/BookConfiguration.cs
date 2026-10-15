using CseLibrary.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CseLibrary.Data.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).UseHiLo("Books_HiLo"); // D-29
        builder.Property(b => b.Isbn).HasMaxLength(20).Audited();
        builder.Property(b => b.Title).IsRequired().HasMaxLength(300).Audited();
        builder.Property(b => b.Authors).HasMaxLength(300).Audited();
        builder.Property(b => b.Publisher).HasMaxLength(200).Audited();
        builder.Property(b => b.Edition).HasMaxLength(50).Audited();
        builder.Property(b => b.PublicationYear).Audited();
        builder.Property(b => b.Category).HasMaxLength(100).Audited();
        builder.Property(b => b.Description).HasMaxLength(2000).Audited();
        builder.ClassifyBaseColumns();
        builder.HasIndex(b => b.Isbn).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter + " AND [Isbn] IS NOT NULL");
        builder.HasIndex(b => b.Title);
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
        builder.ClassifyBaseColumns();
        builder.HasIndex(c => c.AccessionNumber).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter);
        builder.HasOne(c => c.Book).WithMany(b => b.Copies).HasForeignKey(c => c.BookId).OnDelete(DeleteBehavior.Restrict);
    }
}
