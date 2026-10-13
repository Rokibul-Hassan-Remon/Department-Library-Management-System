using CseLibrary.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CseLibrary.Data.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName).HasMaxLength(200);
        // Users are not soft-deleted (UserManager deletes them), so no query filter; Inactive blocks login.
        builder.ToTable(t => t.HasCheckConstraint("CK_AspNetUsers_Status", ApplicationDbContext.StatusCheck));
    }
}

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(r => r.Description).HasMaxLength(500);
    }
}
