using CseLibrary.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CseLibrary.Data.Data.Configurations;

/// <summary>
/// D-23: every property of an <see cref="Core.Interfaces.IAuditable"/> entity is classified explicitly; default = not
/// audited. A model annotation keeps Core free of audit attributes; test C6 fails on a missing classification.
/// </summary>
public static class AuditClassification
{
    public const string Annotation = "CseLibrary:Audited";

    public static PropertyBuilder<T> Audited<T>(this PropertyBuilder<T> property) => property.HasAnnotation(Annotation, true);

    public static PropertyBuilder<T> NotAudited<T>(this PropertyBuilder<T> property) => property.HasAnnotation(Annotation, false);

    /// <summary>Status is audited; the key is the audit row's EntityId and the row columns repeat the audit row itself.</summary>
    public static void ClassifyBaseColumns<T>(this EntityTypeBuilder<T> builder) where T : BaseEntity
    {
        builder.Property(e => e.Id).NotAudited();
        builder.Property(e => e.Status).Audited();
        builder.Property(e => e.CreatedAt).NotAudited();
        builder.Property(e => e.CreatedBy).NotAudited();
        builder.Property(e => e.UpdatedAt).NotAudited();
        builder.Property(e => e.UpdatedBy).NotAudited();
    }

    /// <summary>Concurrency tokens, binary data and shadow properties are never audited, whatever the classification.</summary>
    public static bool IsAudited(IReadOnlyProperty property) =>
        property.FindAnnotation(Annotation)?.Value is true
        && !property.IsConcurrencyToken && !property.IsShadowProperty() && property.ClrType != typeof(byte[]);
}
