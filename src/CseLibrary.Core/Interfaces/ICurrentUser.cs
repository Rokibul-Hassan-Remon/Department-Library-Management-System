namespace CseLibrary.Core.Interfaces;

/// <summary>The signed-in user, or null for system work (startup seeding, background jobs).</summary>
public interface ICurrentUser
{
    string? UserId { get; }
}
