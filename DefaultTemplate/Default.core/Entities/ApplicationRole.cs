using Microsoft.AspNetCore.Identity;

namespace Default.core.Entities;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
