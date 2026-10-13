namespace CseLibrary.Core.DTOs;

public class PermissionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Module { get; set; }
    public string? Description { get; set; }
}

public class CreatePermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Module { get; set; }
    public string? Description { get; set; }
}

public class UpdatePermissionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Module { get; set; }
    public string? Description { get; set; }
}
