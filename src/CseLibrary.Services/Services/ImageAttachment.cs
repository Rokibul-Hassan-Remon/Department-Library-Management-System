using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Results;

namespace CseLibrary.Services.Services;

/// <summary>Picture rules shared by book covers, author photos and user photos (archive: type, size, safe name).</summary>
public static class ImageAttachment
{
    public const int MaxBytes = 2 * 1024 * 1024;

    /// <summary>
    /// The type comes from the file's first bytes, never from its name or the browser's content type,
    /// so a renamed .exe or .svg (script) is refused. Returns the extension to store it under.
    /// </summary>
    public static string? DetectExtension(ReadOnlySpan<byte> head)
    {
        if (head.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF])) return ".jpg";
        if (head.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])) return ".png";
        if (head.Length >= 12 && head[..4].SequenceEqual("RIFF"u8) && head[8..12].SequenceEqual("WEBP"u8)) return ".webp";
        return null;
    }

    /// <summary>
    /// For a record created together with its picture: stores the picture first, passes its path (or null) to
    /// <paramref name="create"/>, and deletes the file again when the create fails.
    /// </summary>
    public static async Task<Result> CreateWithAsync(IImageStore store, ImageKind kind, ImageUpload? image,
        Func<string?, Task<Result>> create, CancellationToken ct)
    {
        string? path = null;
        if (image != null)
        {
            var stored = await StoreAsync(store, kind, image, ct);
            if (!stored.Succeeded) return stored;
            path = stored.Value;
        }

        Result created;
        try
        {
            created = await create(path);
        }
        catch
        {
            store.Delete(path);
            throw;
        }
        if (!created.Succeeded) store.Delete(path);
        return created;
    }

    private static async Task<Result<string>> StoreAsync(IImageStore store, ImageKind kind, ImageUpload image, CancellationToken ct)
    {
        if (image.Length > MaxBytes) return Result<string>.Failure(ErrorCodes.InvalidState, TooLarge);
        using var buffer = new MemoryStream();
        await image.Content.CopyToAsync(buffer, ct);
        if (buffer.Length > MaxBytes) return Result<string>.Failure(ErrorCodes.InvalidState, TooLarge); // the declared length is the client's word
        var bytes = buffer.ToArray();
        var extension = DetectExtension(bytes);
        if (extension == null) return Result<string>.Failure(ErrorCodes.InvalidState, NotAPicture);
        return Result<string>.Success(await store.SaveAsync(kind, bytes, extension, ct));
    }

    /// <summary>
    /// Validates and stores <paramref name="image"/> (null = remove), lets <paramref name="apply"/> put the new path on
    /// the entity and return the old one, saves, then deletes the file no longer referenced: the old one after a
    /// successful save, the new one after a failed save.
    /// </summary>
    public static async Task<Result> ReplaceAsync(IImageStore store, ImageKind kind, ImageUpload? image,
        Func<string?, string?> apply, Func<Task<Result>> save, CancellationToken ct)
    {
        string? newPath = null;
        if (image != null)
        {
            var stored = await StoreAsync(store, kind, image, ct);
            if (!stored.Succeeded) return stored;
            newPath = stored.Value;
        }

        var oldPath = apply(newPath);
        Result saved;
        try
        {
            saved = await save();
        }
        catch
        {
            store.Delete(newPath);
            throw;
        }
        store.Delete(saved.Succeeded ? oldPath : newPath);
        return saved;
    }

    private const string TooLarge = "The picture is larger than 2 MB.";
    private const string NotAPicture = "Choose a JPG, PNG or WebP picture.";
}
