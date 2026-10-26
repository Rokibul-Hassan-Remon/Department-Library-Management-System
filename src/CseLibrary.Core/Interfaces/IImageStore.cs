namespace CseLibrary.Core.Interfaces;

/// <summary>Folder under <c>Uploads/</c>; one per kind of picture.</summary>
public enum ImageKind
{
    Books,
    Authors,
    Users
}

/// <summary>An uploaded file as the service sees it; the web layer wraps the form file.</summary>
public record ImageUpload(Stream Content, long Length);

/// <summary>
/// Pictures live in files, not the database; the entity keeps the path relative to the web project
/// (<c>Uploads/{kind}/{guid}.{ext}</c>). Implemented by the web layer, which knows the folder.
/// </summary>
public interface IImageStore
{
    /// <summary>Writes already validated bytes under a new random name and returns its relative path.</summary>
    Task<string> SaveAsync(ImageKind kind, byte[] content, string extension, CancellationToken ct = default);

    /// <summary>Removes the file; a null, unknown or already missing path is ignored.</summary>
    void Delete(string? relativePath);
}
