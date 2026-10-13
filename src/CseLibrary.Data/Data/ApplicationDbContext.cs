using System.Linq.Expressions;
using CseLibrary.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<GeneralConfig> GeneralConfigs => Set<GeneralConfig>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // D-51: one filter hides soft-deleted rows of every BaseEntity, plus the tinyint CHECK.
        foreach (var type in builder.Model.GetEntityTypes().Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            var e = Expression.Parameter(type.ClrType, "e");
            var notDeleted = Expression.NotEqual(
                Expression.Property(e, nameof(BaseEntity.Status)), Expression.Constant(RecordStatus.Deleted));
            builder.Entity(type.ClrType).HasQueryFilter(Expression.Lambda(notDeleted, e));
            builder.Entity(type.ClrType).ToTable(t => t.HasCheckConstraint($"CK_{type.GetTableName()}_Status", StatusCheck));
        }
    }

    internal const string StatusCheck = "[Status] IN (1, 2, 3)";

    /// <summary>Filter for unique indexes, so a deleted row's key can be reused (D-51).</summary>
    internal const string NotDeletedFilter = "[Status] <> 3";
}
