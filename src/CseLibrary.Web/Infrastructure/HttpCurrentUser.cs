using System.Security.Claims;
using CseLibrary.Core.Interfaces;

namespace CseLibrary.Web.Infrastructure;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? UserId => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
