using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebApplication2.Models;
using WebApplication2.Services;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication2.Controllers;

[Authorize(Roles = "Customer")]
public class TransactionsController : Controller
{
    private readonly BankApiClient _bankApiClient;

    public TransactionsController(BankApiClient bankApiClient) => _bankApiClient = bankApiClient;

    [HttpGet]
    public async Task<IActionResult> Index(string? transactionType, string? sortOrder)
    {
        var transactions = await _bankApiClient.GetAsync<List<BankTrans>>("api/transactions", new()
        {
            ["transactionType"] = transactionType,
            ["sortOrder"] = sortOrder
        });

        return View("Index", transactions.Data ?? new List<BankTrans>());
    }

    [HttpGet]
    public async Task<IActionResult> Search(string accountNumber, string? transactionType, string? sortOrder)
    {
        var transactions = await _bankApiClient.GetAsync<List<BankTrans>>("api/transactions/search", new()
        {
            ["accountNumber"] = accountNumber,
            ["transactionType"] = transactionType,
            ["sortOrder"] = sortOrder
        });

        return View("Index", transactions.Data ?? new List<BankTrans>());
    }
}