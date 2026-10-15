using CseLibrary.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CseLibrary.Data.Data.Configurations;

public class GeneralConfigConfiguration : IEntityTypeConfiguration<GeneralConfig>
{
    public void Configure(EntityTypeBuilder<GeneralConfig> builder)
    {
        builder.ToTable("GeneralConfigs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseHiLo("GeneralConfigs_HiLo"); // D-29: id known at AddAsync, for same-SaveChanges audit rows
        builder.Property(c => c.Key).IsRequired().HasMaxLength(150).Audited();
        builder.Property(c => c.Value).HasMaxLength(2000).Audited(); // never a secret here (D-40, Session 22 Q1)
        builder.Property(c => c.Description).HasMaxLength(500).Audited();
        builder.Property(c => c.Group).HasMaxLength(100).Audited();
        builder.ClassifyBaseColumns();
        builder.HasIndex(c => c.Key).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter);
    }
}
