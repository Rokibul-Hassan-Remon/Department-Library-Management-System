namespace CseLibrary.Core.Entities;

/// <summary>Audit columns are set by the Data layer's SaveChanges interceptor, never by services (D-50).</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
