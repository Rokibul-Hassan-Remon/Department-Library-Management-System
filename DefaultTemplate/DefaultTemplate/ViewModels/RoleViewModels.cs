using System.ComponentModel.DataAnnotations;
using Default.core.DTOs;

namespace DefaultTemplate.ViewModels;

public class CreateRoleViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public IList<int> SelectedPermissionIds { get; set; } = new List<int>();
    public IList<PermissionDto> AvailablePermissions { get; set; } = new List<PermissionDto>();
}

public class EditRoleViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public IList<int> SelectedPermissionIds { get; set; } = new List<int>();
    public IList<PermissionDto> AvailablePermissions { get; set; } = new List<PermissionDto>();
}
