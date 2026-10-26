using CseLibrary.Core.Constants;
using CseLibrary.Web.Authorization;
using CseLibrary.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers;

/// <summary>
/// Serves uploaded pictures (archive: access to uploads is restricted). Book covers and author photos:
/// any signed-in user. User photos: the user, or staff who can view users or library members.
/// </summary>
[Authorize]
[Route("Media")]
public class MediaController(ImageStore store, IAuthorizationService authorization) : Controller
{
    [HttpGet("{kind}/{file}")]
    public async Task<IActionResult> Get(string kind, string file)
    {
        var relative = $"Uploads/{kind}/{file}";
        var full = store.GetFullPath(relative);
        if (full == null || !System.IO.File.Exists(full)) return NotFound();

        if (kind == "Users"
            && User.FindFirst(AppClaims.Photo)?.Value != relative
            && !await authorization.CanAsync(User, Permissions.Users.View)
            && !await authorization.CanAsync(User, Permissions.LibraryMembers.View))
            return NotFound(); // not Forbid: do not confirm that someone else's photo exists

        // A stored name is never reused (new GUID per upload), so the browser may keep it.
        Response.Headers.CacheControl = "private, max-age=31536000, immutable";
        return PhysicalFile(full, Path.GetExtension(full) switch { ".png" => "image/png", ".webp" => "image/webp", _ => "image/jpeg" });
    }
}
