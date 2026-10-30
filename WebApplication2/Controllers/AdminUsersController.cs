using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using RestSharp;
using System.Security.AccessControl;
using System.Security.Claims;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminUsersController : Controller
    {
        private readonly BankApiClient _bankApiClient;

        public AdminUsersController(BankApiClient bankApiClient)
        {
            _bankApiClient = bankApiClient;
        }

        private int AdminId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private Task Audit(string action, string details)
        {
            return _bankApiClient.SendAsync<Object>(
                Method.Post,
                "api/audit",
                new
                {
                    adminUserId = AdminId,
                    action = action,
                    details = details
                });
        }

        private void AddApiErrors<T>(ApiResult<T> result)
        {
            ModelState.AddModelError("", result.ErrorMessage ?? "Something went wrong");

            if (result.Details != null)
            {
                foreach (var (field, msg) in result.Details)
                {
                    ModelState.AddModelError("", msg);
                }
            }
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var result = await _bankApiClient.GetAsync<UserPageDto>(
                "api/users-list",
                new()
                {
                    ["search"] = search,
                    ["page"] = page.ToString(),
                    ["pageSize"] = "15"
                });

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
            }

            ViewBag.Search = search;

            return View(result.Data ?? new UserPageDto());
        }

        [HttpGet] public IActionResult Create()
        {
            CreateUserViewModel model = new CreateUserViewModel();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _bankApiClient.SendAsync<object>(
                Method.Post,
                "api/admin/users",
                vm);

            if (!result.Success)
            {
                AddApiErrors(result);
                return View(vm);
            }

            await Audit("CreateUser", $"Created {vm.Role} '{vm.Username}'");

            TempData["Message"] = $"User {vm.Username} created";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _bankApiClient.GetAsync<EditUserViewModel>($"api/admin/users/{id}");

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _bankApiClient.SendAsync<object>(
                Method.Put,
                $"api/admin/users/{id}",
                vm);

            if (!result.Success)
            {
                AddApiErrors(result);
                return View(vm);
            }

            await Audit("EditUser", $"Edited user {id} ({vm.Name})");

            TempData["Message"] = "User updated";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            if (id == AdminId)
            {
                TempData["Error"] = "You cant deactivate your own account";
                return RedirectToAction(nameof(Index));
            }

            var result = await _bankApiClient.SendAsync<object>(
                Method.Post,
                $"api/admin/users/{id}/deactivate"
                );

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
            }
            else
            {
                await Audit("DeactivateUser", $"Deactivated user {id}");
                TempData["Message"] = "User deactivated";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ResetPassword(int id)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.UserId = id;

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _bankApiClient.SendAsync<object>(
                Method.Post,
                $"api/admin/users/{vm.UserId}/reset-password",
                new
                {
                    newPassword = vm.NewPassword
                });

            if (!result.Success)
            {
                AddApiErrors(result);
                return View(vm);
            }

            await Audit("ResetPassword", $"Reset password for user {vm.UserId}");

            TempData["Message"] = "Password reset";

            return RedirectToAction(nameof(Index));
        }
    }
}
