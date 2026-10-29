using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Customer")]
    public class AccountsController : Controller
    {
        private readonly BankApiClient _bankApiClient;

        public AccountsController(BankApiClient bankApiClient)
        {
            _bankApiClient = bankApiClient;
        }

        private int CurrentUserId => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        public async Task<IActionResult> Index()
        {
            var result = await _bankApiClient.GetAsync<List<AccountSummaryDto>>($"api/users/{CurrentUserId}/accounts");

            if (!result.Success)
            {
                ViewData["Error"] = result.ErrorMessage;
                return View(new List<AccountSummaryDto>());
            }

            return View(result.Data ?? new List<AccountSummaryDto>());
        }

        public async Task<IActionResult> History(
            string accountNumber,
            DateTime startDate,
            DateTime endDate,
            string sortBy = "date",
            string sortOrder = "desc",
            int page = 1,
            int pageSize = 10)
        {
            var result = await _bankApiClient.GetAsync<HistoryResponseDto>(
                $"api/accounts/{CurrentUserId}/history",
                new()
                {
                    ["accountNumber"] = accountNumber,
                    ["startDate"] = startDate.ToString(),
                    ["endDate"] = endDate.ToString(),
                    ["sortBy"] = sortBy,
                    ["sortOrder"] = sortOrder,
                    ["page"] = page.ToString(),
                    ["pageSize"] = pageSize.ToString()
                });

            if (!result.Success)
            {
                ViewData["Error"] = result.ErrorMessage;
            }

            return View(
                new HistoryViewModel
                {
                    AccountNumber = accountNumber,
                    StartDate = startDate,
                    EndDate = endDate,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    Data = result.Data ?? new HistoryResponseDto
                    {
                        Pagination = new PageDto
                        {
                            Page = page,
                            PageSize = pageSize
                        }
                    }
                });
        }
    }
}
