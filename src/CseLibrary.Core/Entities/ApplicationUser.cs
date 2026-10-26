using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    /// <summary>Relative to the web project, e.g. <c>Uploads/Users/{guid}.jpg</c>.</summary>
    public string? PhotoPath { get; set; }
}
