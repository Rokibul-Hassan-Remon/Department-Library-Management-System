using System.Text.RegularExpressions;
using CseLibrary.Core.Interfaces;

namespace CseLibrary.Web.Infrastructure;

/// <summary>
/// Files under <c>{base}/Uploads/{Books|Authors|Users}/{guid}.{ext}</c>, outside wwwroot so they are served only
/// through <see cref="Controllers.MediaController"/>. The database keeps the path from <c>Uploads/</c> on.
/// Base = <c>Uploads:BasePath</c> (tests) or the web project's content root.
/// </summary>
public partial class ImageStore(IWebHostEnvironment env, IConfiguration configuration) : IImageStore
{
    private readonly string _base = configuration["Uploads:BasePath"] ?? env.ContentRootPath;

    public async Task<string> SaveAsync(ImageKind kind, byte[] content, string extension, CancellationToken ct = default)
    {
        var relative = $"Uploads/{kind}/{Guid.NewGuid():N}{extension}";
        var full = Path.Combine(_base, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await File.WriteAllBytesAsync(full, content, ct);
        return relative;
    }

    public void Delete(string? relativePath)
    {
        if (GetFullPath(relativePath) is { } full) File.Delete(full); // no-op when the file is already gone
    }

    /// <summary>The file for a stored path, or null when the path is not one this store writes (no traversal).</summary>
    public string? GetFullPath(string? relativePath) =>
        relativePath != null && StoredPath().IsMatch(relativePath) ? Path.Combine(_base, relativePath) : null;

    [GeneratedRegex(@"^Uploads/(Books|Authors|Users)/[0-9a-f]{32}\.(jpg|png|webp)$")]
    private static partial Regex StoredPath();
}
