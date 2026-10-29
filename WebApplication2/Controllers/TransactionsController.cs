using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class TransactionsController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(string? transactionType, string? sortOrder)
        {
            RestClient restClient = new RestClient("https://localhost:7285");

            RestRequest restRequest = new RestRequest("api/transactions", Method.Get);

            if (!string.IsNullOrEmpty(transactionType))
            {
                restRequest.AddQueryParameter("transactionType", transactionType);
            }

            if (!string.IsNullOrEmpty(sortOrder))
            {
                restRequest.AddQueryParameter("sortOrder", sortOrder);
            }

            List<BankTrans>? transactions = await restClient.GetAsync<List<BankTrans>>(restRequest);

            if (transactions == null)
            {
                return View("Index", new List<BankTrans>());
            }

            return View("Index", transactions);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string accountNumber, string? transactionType, string? sortOrder)
        {
            RestClient restClient = new RestClient("https://localhost:7285");

            RestRequest restRequest = new RestRequest("api/transactions/search", Method.Get);

            restRequest.AddQueryParameter("accountNumber", accountNumber);

            if (!string.IsNullOrEmpty(transactionType))
            {
                restRequest.AddQueryParameter("transactionType", transactionType);
            }

            if (!string.IsNullOrEmpty(sortOrder))
            {
                restRequest.AddQueryParameter("sortOrder", sortOrder);
            }

            List<BankTrans>? transactions = await restClient.GetAsync<List<BankTrans>>(restRequest);

            if (transactions == null)
            {
                return View("Index", new List<BankTrans>());
            }

            return View("Index", transactions);
        }
    }
}