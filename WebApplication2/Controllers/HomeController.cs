using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // TEMPORARY: delete before submission
        [AllowAnonymous]
        public async Task<IActionResult> DevAdmin([FromServices] IWebHostEnvironment env)
        {
            if (!env.IsDevelopment()) return NotFound();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "26"),   // admin's ID: check with SELECT ID FROM Users WHERE Username='admin'
                new(ClaimTypes.Name, "Bank Admin"),
                new(ClaimTypes.Role, "Administrator")
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
            return RedirectToAction("Index", "AdminUsers");
        }
    }
}
