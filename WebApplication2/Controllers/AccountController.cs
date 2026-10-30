using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers;

public class AccountController : Controller
{
    private readonly BankApiClient _bankApiClient;

    public AccountController(BankApiClient bankApiClient)
    {
        _bankApiClient = bankApiClient;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [AllowAnonymous]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _bankApiClient.SendAsync<object>(Method.Post, "api/users", new
        {
            username = model.Username,
            name = model.Name,
            usrEmail = model.Email,
            usrAddress = model.Address,
            usrPhone = model.Phone,
            password = model.Password
        });

        if (!result.Success)
        {
            AddApiError(result.ErrorMessage ?? "Registration failed.");
            return View(model);
        }

        return RedirectToAction(nameof(Login), new { username = model.Username });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null, string? username = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl, Username = username ?? string.Empty });
    }

    [AllowAnonymous]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _bankApiClient.SendAsync<LoginResponse>(Method.Post, "api/users/loginUser", new
        {
            username = model.Username,
            password = model.Password
        });

        if (!result.Success || result.Data == null)
        {
            AddApiError(result.ErrorMessage ?? "Invalid username or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Data.ID.ToString()),
            new(ClaimTypes.Name, result.Data.Username),
            new(ClaimTypes.Role, result.Data.Role)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = false });

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private void AddApiError(string message)
    {
        ModelState.AddModelError(string.Empty, message);
    }
}
