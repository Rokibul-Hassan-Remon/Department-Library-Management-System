namespace CseLibrary.Core.Entities;

/// <summary>
/// One append-only audit row (D-21). Services set only the category, entity, action and changes;
/// the Data layer fills actor, time and correlation for every new row.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }
    public AuditCategory Category { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? ActorUserId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string? Changes { get; set; }
}

/// <summary>Stored as tinyint; new values are appended, never renumbered.</summary>
public enum AuditCategory : byte
{
    Entity = 1,
    Security = 2
}
