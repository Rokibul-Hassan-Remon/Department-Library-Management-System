using CseLibrary.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services;

/// <summary>D-56 Q-S21-1: a registration default role must exist and must not be privileged (Q-S21-5).</summary>
internal static class DefaultRole
{
    /// <summary>The role's stored name, or null when <paramref name="name"/> is empty or not a usable default.</summary>
    public static async Task<string?> ResolveAsync(IUnitOfWork uow, ILookupNormalizer normalizer, string? name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var role = (await uow.Roles.GetNamesAsync(ct)).FirstOrDefault(r => string.Equals(r, name.Trim(), StringComparison.OrdinalIgnoreCase));
        return role != null && !await uow.Roles.AnyPrivilegedAsync([normalizer.NormalizeName(role)], ct) ? role : null;
    }
}
