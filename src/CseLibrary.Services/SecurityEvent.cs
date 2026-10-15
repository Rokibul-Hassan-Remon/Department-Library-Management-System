using System.Text.Json;
using CseLibrary.Core.Entities;

namespace CseLibrary.Services;

/// <summary>
/// D-24: a Category = Security audit row; the Data layer adds actor, time and correlation. Details hold names and ids
/// only — never passwords, hashes or tokens.
/// </summary>
internal static class SecurityEvent
{
    public const string User = "User";
    public const string Role = "Role";

    public static AuditLog For(string entityType, string entityId, string action, object? details = null) => new()
    {
        Category = AuditCategory.Security,
        EntityType = entityType,
        EntityId = entityId,
        Action = action,
        Changes = details is null ? null : JsonSerializer.Serialize(details)
    };
}
