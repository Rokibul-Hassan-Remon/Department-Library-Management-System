using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Tests.Infrastructure;

/// <summary>
/// Test-only (A11): carries no authorization metadata, so only the fallback policy guards it.
/// <see cref="Throw"/> raises an unexpected exception for B1.
/// </summary>
public class FallbackProbeController : Controller
{
    public const string SecretDetail = "probe-secret-detail";

    public IActionResult Index() => Content("reached");

    public IActionResult Throw() => throw new InvalidOperationException(SecretDetail);
}
