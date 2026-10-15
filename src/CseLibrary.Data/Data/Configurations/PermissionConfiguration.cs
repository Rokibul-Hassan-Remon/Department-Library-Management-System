using CseLibrary.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CseLibrary.Data.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseHiLo("Permissions_HiLo"); // D-29
        builder.Property(p => p.Name).IsRequired().HasMaxLength(150).Audited();
        builder.Property(p => p.DisplayName).HasMaxLength(200).Audited();
        builder.Property(p => p.Module).HasMaxLength(100).Audited();
        builder.Property(p => p.Description).HasMaxLength(500).Audited();
        builder.ClassifyBaseColumns();
        builder.HasIndex(p => p.Name).IsUnique().HasFilter(ApplicationDbContext.NotDeletedFilter);
    }
}
