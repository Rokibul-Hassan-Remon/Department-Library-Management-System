namespace CseLibrary.Core.DTOs;

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<int> PermissionIds { get; set; } = new List<int>();
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IList<int> PermissionIds { get; set; } = new List<int>();
}

public class UpdateRoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IList<int> PermissionIds { get; set; } = new List<int>();
}
