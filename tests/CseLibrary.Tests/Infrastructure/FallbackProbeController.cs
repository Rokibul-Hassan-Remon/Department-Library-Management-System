using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Tests.Infrastructure;

/// <summary>Test-only (A11): carries no authorization metadata, so only the fallback policy guards it.</summary>
public class FallbackProbeController : Controller
{
    public IActionResult Index() => Content("reached");
}
