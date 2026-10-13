using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;

namespace CseLibrary.Services.Mapping;

internal static class DtoMappings
{
    public static UserDto ToDto(this ApplicationUser u) => new()
    {
        Id = u.Id,
        UserName = u.UserName,
        Email = u.Email,
        FullName = u.FullName,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };

    public static RoleDto ToDto(this ApplicationRole r) => new()
    {
        Id = r.Id,
        Name = r.Name ?? string.Empty,
        Description = r.Description,
        CreatedAt = r.CreatedAt,
        PermissionIds = r.RolePermissions.Select(rp => rp.PermissionId).ToList()
    };

    public static PermissionDto ToDto(this Permission p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        DisplayName = p.DisplayName,
        Module = p.Module,
        Description = p.Description
    };

    public static GeneralConfigDto ToDto(this GeneralConfig c) => new()
    {
        Id = c.Id,
        Key = c.Key,
        Value = c.Value,
        Description = c.Description,
        Group = c.Group
    };
}
