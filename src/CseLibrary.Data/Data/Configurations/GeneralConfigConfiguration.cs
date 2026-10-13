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
        builder.Property(c => c.Key).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Value).HasMaxLength(2000);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Group).HasMaxLength(100);
        builder.HasIndex(c => c.Key).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter);
    }
}
