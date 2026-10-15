using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CseLibrary.Web.Infrastructure;

/// <summary>
/// D-55 (Q-S19-10): a permission failure sends the user back to the local page they came from (else Home) with a
/// red toast instead of the AccessDenied page; AJAX requests get a plain 403. The server still refuses the request.
/// </summary>
public static class AccessDeniedRedirect
{
    public const string Message = "You don't have permission to perform this action.";

    public static Task HandleAsync(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (context.Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        var tempData = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(context.HttpContext);
        tempData["Error"] = Message;
        tempData.Save();
        context.Response.Redirect(ReturnUrl(context.Request));
        return Task.CompletedTask;
    }

    // Only the Referer of this same site, never the denied URL itself (no loop), never "//host" (open redirect).
    internal static string ReturnUrl(HttpRequest request)
    {
        if (Uri.TryCreate(request.Headers.Referer.ToString(), UriKind.Absolute, out var referer)
            && referer.Scheme == request.Scheme
            && string.Equals(referer.Authority, request.Host.Value, StringComparison.OrdinalIgnoreCase))
        {
            var local = referer.PathAndQuery;
            if (!local.StartsWith("//") && !local.StartsWith("/\\") && local != request.Path + request.QueryString)
                return local;
        }
        return "/";
    }
}
