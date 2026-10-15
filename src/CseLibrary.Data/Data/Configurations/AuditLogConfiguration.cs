using CseLibrary.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CseLibrary.Data.Data.Configurations;

/// <summary>D-21 (Session 22 P1): append-only, identity key, three lookup indexes.</summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(64);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
        builder.Property(a => a.ActorUserId).HasMaxLength(450);
        builder.Property(a => a.CorrelationId).HasMaxLength(64);
        builder.HasIndex(a => new { a.EntityType, a.EntityId, a.OccurredAtUtc });
        builder.HasIndex(a => new { a.ActorUserId, a.OccurredAtUtc });
        builder.HasIndex(a => a.CorrelationId);
    }
}
