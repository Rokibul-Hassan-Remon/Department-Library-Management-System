using System.Security.Claims;
using CseLibrary.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CseLibrary.Tests.Infrastructure;

/// <summary>
/// Inside a request: the signed-in user, as in the app. Outside one (direct service calls in tests): the user set by
/// <see cref="ActAs"/>, else the seeded Admin, so test setup passes the D-56 Admin-only checks unless a test says otherwise.
/// </summary>
public sealed class TestCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private static readonly AsyncLocal<string?> Acting = new();

    /// <summary>Set once the host has seeded; startup seeding itself runs as no user.</summary>
    public static string? SeedAdminId { get; set; }

    public string? UserId => accessor.HttpContext is { } http
        ? http.User.FindFirstValue(ClaimTypes.NameIdentifier)
        : Acting.Value ?? SeedAdminId;

    /// <summary>Direct service calls in the calling async flow run as <paramref name="userId"/> until disposed.</summary>
    public static IDisposable ActAs(string userId)
    {
        var previous = Acting.Value;
        Acting.Value = userId;
        return new Restore(previous);
    }

    private sealed class Restore(string? previous) : IDisposable
    {
        public void Dispose() => Acting.Value = previous;
    }
}
