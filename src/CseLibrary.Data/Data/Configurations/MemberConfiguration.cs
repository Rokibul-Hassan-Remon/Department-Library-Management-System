using CseLibrary.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CseLibrary.Data.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members", t => t.HasCheckConstraint("CK_Members_MemberType", "[MemberType] IN (1, 2)"));
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).UseHiLo("Members_HiLo"); // D-29
        builder.Property(m => m.MemberCode).IsRequired().HasMaxLength(30).Audited();
        builder.Property(m => m.FullName).IsRequired().HasMaxLength(150).Audited();
        builder.Property(m => m.MemberType).Audited();
        builder.Property(m => m.Email).HasMaxLength(256).Audited();
        builder.Property(m => m.Phone).HasMaxLength(30).Audited();
        builder.ClassifyBaseColumns();
        builder.HasIndex(m => m.MemberCode).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter);
        builder.HasIndex(m => m.FullName);
    }
}

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).UseHiLo("Loans_HiLo"); // D-29
        builder.Property(l => l.MemberId).Audited();
        builder.Property(l => l.BookCopyId).Audited();
        builder.Property(l => l.IssuedAtUtc).Audited();
        builder.Property(l => l.DueDate).Audited();
        builder.Property(l => l.ReturnedAtUtc).Audited();
        builder.Property(l => l.RenewCount).Audited();
        builder.ClassifyBaseColumns();
        // D-35: one active loan per copy, so two concurrent issues of a copy cannot both commit.
        builder.HasIndex(l => l.BookCopyId).IsUnique().HasFilter("[ReturnedAtUtc] IS NULL AND " + ApplicationDbContext.NotDeletedFilter)
            .HasDatabaseName("UX_Loans_ActiveCopy");
        builder.HasIndex(l => new { l.MemberId, l.ReturnedAtUtc });
        builder.HasIndex(l => l.DueDate).HasFilter("[ReturnedAtUtc] IS NULL");
        builder.HasOne(l => l.Member).WithMany(m => m.Loans).HasForeignKey(l => l.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.BookCopy).WithMany().HasForeignKey(l => l.BookCopyId).OnDelete(DeleteBehavior.Restrict);
    }
}
