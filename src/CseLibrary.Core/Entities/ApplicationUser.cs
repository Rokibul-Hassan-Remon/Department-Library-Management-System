using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
