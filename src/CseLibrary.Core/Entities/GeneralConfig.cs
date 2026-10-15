using CseLibrary.Core.Interfaces;

namespace CseLibrary.Core.Entities;

public class GeneralConfig : BaseEntity, IAuditable
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string? Group { get; set; }
}
