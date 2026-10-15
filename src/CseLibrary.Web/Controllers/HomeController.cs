using System.Diagnostics;
using CseLibrary.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CseLibrary.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // D-34: the Reference ID is the request's TraceId, which Serilog writes on every log line of the request.
            var requestId = Activity.Current?.TraceId.ToHexString() ?? HttpContext.TraceIdentifier;
            TempData["Error"] = $"An unexpected error occurred. Reference ID: {requestId}"; // D-55 Q-S19-11
            return View(new ErrorViewModel { RequestId = requestId });
        }

        // D-34: friendly page for 4xx/5xx responses without a body (UseStatusCodePagesWithReExecute).
        // AJAX callers keep the bare status (D-55: 403 stays a plain 403).
        [AllowAnonymous]
        public IActionResult HttpStatus(int id)
        {
            if (Request.Headers.XRequestedWith == "XMLHttpRequest") return new EmptyResult();
            return View(id);
        }
    }
}
