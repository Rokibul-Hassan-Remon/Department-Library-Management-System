using CseLibrary.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CseLibrary.Data.Data.Configurations;

public class LibraryMemberConfiguration : IEntityTypeConfiguration<LibraryMember>
{
    public void Configure(EntityTypeBuilder<LibraryMember> builder)
    {
        builder.ToTable("LibraryMembers", t =>
        {
            t.HasCheckConstraint("CK_LibraryMembers_MemberType", "[MemberType] IN (1, 2)");
            t.HasCheckConstraint("CK_LibraryMembers_Approval", "[Approval] IN (1, 2, 3)");
        });
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).UseHiLo("LibraryMembers_HiLo"); // D-29
        builder.Property(m => m.MemberCode).IsRequired().HasMaxLength(30).Audited();
        builder.Property(m => m.FullName).IsRequired().HasMaxLength(150).Audited();
        builder.Property(m => m.MemberType).Audited();
        builder.Property(m => m.Email).HasMaxLength(254).Audited();
        builder.Property(m => m.Phone).HasMaxLength(30).Audited();
        builder.Property(m => m.IdentityUserId).HasMaxLength(450).Audited();
        builder.Property(m => m.Approval).Audited();
        builder.ClassifyBaseColumns();
        builder.HasIndex(m => m.MemberCode).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter);
        builder.HasIndex(m => m.FullName);
        builder.HasIndex(m => m.IdentityUserId).IsUnique().HasFilter("[IdentityUserId] IS NOT NULL");
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(m => m.IdentityUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("LendingTransactions", t =>
        {
            t.HasCheckConstraint("CK_LendingTransactions_Status",
                "([TransactionStatus] = 1 AND [ReturnDateUtc] IS NULL) OR ([TransactionStatus] = 2 AND [ReturnDateUtc] IS NOT NULL)");
            t.HasCheckConstraint("CK_LendingTransactions_ReturnCondition", "[ReturnCondition] IS NULL OR [ReturnCondition] IN (1, 2)");
        });
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).UseHiLo("LendingTransactions_HiLo"); // D-29
        builder.Property(l => l.LibraryMemberId).Audited();
        builder.Property(l => l.BookCopyId).Audited();
        builder.Property(l => l.IssueDateUtc).Audited();
        builder.Property(l => l.DueDate).Audited();
        builder.Property(l => l.ReturnDateUtc).Audited();
        builder.Property(l => l.TransactionStatus).Audited();
        builder.Property(l => l.IssuedByUserId).IsRequired().HasMaxLength(450).Audited();
        builder.Property(l => l.ReturnedByUserId).HasMaxLength(450).Audited();
        builder.Property(l => l.ReturnCondition).Audited();
        builder.Property(l => l.RenewCount).Audited();
        builder.ClassifyBaseColumns();
        // D-35 / archive §5: one Issued transaction per copy, so two concurrent issues of a copy cannot both commit.
        builder.HasIndex(l => l.BookCopyId).IsUnique().HasFilter("[TransactionStatus] = 1").HasDatabaseName("UX_Lending_ActiveBookCopy");
        builder.HasIndex(l => new { l.LibraryMemberId, l.TransactionStatus });
        builder.HasIndex(l => l.DueDate).HasFilter("[TransactionStatus] = 1");
        builder.HasOne(l => l.LibraryMember).WithMany(m => m.Loans).HasForeignKey(l => l.LibraryMemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.BookCopy).WithMany().HasForeignKey(l => l.BookCopyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(l => l.IssuedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(l => l.ReturnedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LibraryPolicyConfiguration : IEntityTypeConfiguration<LibraryPolicy>
{
    public void Configure(EntityTypeBuilder<LibraryPolicy> builder)
    {
        builder.ToTable("LibraryPolicies", t => t.HasCheckConstraint("CK_LibraryPolicies_Values",
            "[MaxActiveLoans] >= 0 AND [LoanDays] >= 1 AND [MaxRenewals] >= 0"));
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseHiLo("LibraryPolicies_HiLo");
        builder.Property(p => p.MemberCategory).Audited();
        builder.Property(p => p.MaxActiveLoans).Audited();
        builder.Property(p => p.LoanDays).Audited();
        builder.Property(p => p.MaxRenewals).Audited();
        builder.ClassifyBaseColumns();
        builder.HasIndex(p => p.MemberCategory).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter);
    }
}

public class EmailNotificationLogConfiguration : IEntityTypeConfiguration<EmailNotificationLog>
{
    public void Configure(EntityTypeBuilder<EmailNotificationLog> builder)
    {
        builder.ToTable("EmailNotificationLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseHiLo("EmailNotificationLogs_HiLo");
        builder.Property(e => e.Recipient).IsRequired().HasMaxLength(254);
        builder.Property(e => e.Subject).IsRequired().HasMaxLength(300);
        builder.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastError).HasMaxLength(1000);
        builder.HasIndex(e => e.IdempotencyKey).IsUnique();
        builder.HasOne(e => e.LendingTransaction).WithMany().HasForeignKey(e => e.LendingTransactionId).OnDelete(DeleteBehavior.Restrict);
    }
}
