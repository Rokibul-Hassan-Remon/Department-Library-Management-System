using CseLibrary.Core.Results;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services;

internal static class IdentityResultExtensions
{
    public static Result ToResult(this IdentityResult r) => r.Succeeded
        ? Result.Success()
        : Result.Failure(ErrorCodes.IdentityFailed, string.Join("; ", r.Errors.Select(e => e.Description)));
}
