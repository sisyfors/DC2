using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using WebApplication2.Models;
using WebApplication2.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication2.Controllers;

[Authorize(Roles = "Customer")]
public class UserProfileController : Controller
{
    private readonly BankApiClient _bankApiClient;

    public UserProfileController(BankApiClient bankApiClient) => _bankApiClient = bankApiClient;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("/UserProfile")]
    public async Task<IActionResult> GetUserProfile()
    {
        var result = await _bankApiClient.GetAsync<User>($"api/users/{CurrentUserId}");
        if (!result.Success)
        {
            ViewBag.Error = result.ErrorMessage;
            return View("UserProfile", new User());
        }

        return View("UserProfile", result.Data ?? new User());
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUser([FromBody] User updatedUser)
    {
        var result = await _bankApiClient.GetAsync<User>($"api/users/{CurrentUserId}");
        if (!result.Success || result.Data == null)
            return StatusCode(502, new { message = "could not retrieve user." });

        var response = await _bankApiClient.SendAsync<object>(Method.Put, $"api/users/{Uri.EscapeDataString(result.Data.UserName)}", new
        {
            Name = result.Data.Name,
            UserEmail = updatedUser.UserEmail,
            UserAddress = updatedUser.UserAddress,
            UserPhone = updatedUser.UserPhone
        });

        return response.Success
            ? Json(new { message = "Profile updated successfully." })
            : StatusCode(502, new { message = response.ErrorMessage ?? "Update failed." });
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest passwordRequest)
    {
        var result = await _bankApiClient.GetAsync<User>($"api/users/{CurrentUserId}");
        if (!result.Success || result.Data == null)
            return StatusCode(502, new { message = "could not retrieve user." });

        var response = await _bankApiClient.SendAsync<object>(Method.Put, $"api/users/{Uri.EscapeDataString(result.Data.UserName)}/password", passwordRequest);

        return response.Success
            ? Json(new { message = "password changed successfully." })
            : BadRequest(new { message = response.ErrorMessage ?? "unable to change password." });
    }
}
