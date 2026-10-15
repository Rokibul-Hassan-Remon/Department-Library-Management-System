using CseLibrary.Core.Interfaces;

namespace CseLibrary.Core.Entities;

public class Permission : BaseEntity, IAuditable
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Module { get; set; }
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
