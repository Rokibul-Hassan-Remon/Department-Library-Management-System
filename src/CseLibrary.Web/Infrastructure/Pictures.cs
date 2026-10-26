using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Infrastructure;

/// <summary>Web side of uploaded pictures: URLs, form files, the request size limit.</summary>
public static class Pictures
{
    /// <summary>A 2 MB picture plus the other form fields.</summary>
    public const long RequestLimit = 3 * 1024 * 1024;

    /// <summary>"Uploads/Books/x.jpg" -> "/Media/Books/x.jpg" (MediaController); null stays null.</summary>
    public static string? Media(this IUrlHelper url, string? storedPath) =>
        storedPath is { Length: > 8 } && storedPath.StartsWith("Uploads/", StringComparison.Ordinal)
            ? url.Content("~/Media/" + storedPath["Uploads/".Length..])
            : null;

    /// <summary>The upload as the services take it; no file (or an empty one) is null.</summary>
    public static ImageUpload? ToImageUpload(this IFormFile? file) =>
        file is { Length: > 0 } ? new ImageUpload(file.OpenReadStream(), file.Length) : null;

    /// <summary>
    /// Saves the picture after its record (_ImageInput: a new file, or "remove" ticked). A refused picture
    /// leaves the record saved and shows a warning instead of losing the rest of the form.
    /// </summary>
    public static async Task SavePictureAsync(this Controller controller, IFormFile? file, bool remove, Func<ImageUpload?, Task<Result>> set)
    {
        var upload = file.ToImageUpload();
        if (upload == null && !remove) return;
        var result = await set(upload);
        if (!result.Succeeded) controller.TempData["Warning"] = $"Saved, but the picture was not: {result.Error}";
    }
}
