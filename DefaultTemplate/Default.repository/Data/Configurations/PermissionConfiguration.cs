using Default.core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Default.repository.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.DisplayName).HasMaxLength(200);
        builder.Property(p => p.Module).HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.HasIndex(p => p.Name).IsUnique();
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
