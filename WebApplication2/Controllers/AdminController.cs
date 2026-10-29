using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly BankApiClient _bankApiClient;

        public AdminController(BankApiClient bankApiClient)
        {
            _bankApiClient = bankApiClient;
        }

        public async Task<IActionResult> DashBoard(int page = 1)
        {
            var result = await _bankApiClient.GetAsync<AuditPageDto>(
                "api/audit",
                new()
                {
                    ["page"] = page.ToString(),
                    ["pageSize"] = "15"
                });

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
            }
            
            if (result.Data == null)
            {
                return View(new AuditPageDto());
            }

            return View(result.Data);
        }

    }
}
