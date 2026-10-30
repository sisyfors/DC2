using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebApplication2.Models;
using Microsoft.AspNetCore.Authorization;
using WebApplication2.Services;

namespace WebApplication2.Controllers;

[Authorize(Roles = "Customer")]
public class TransferController : Controller
{
    private readonly BankApiClient _bankApiClient;

    public TransferController(BankApiClient bankApiClient) => _bankApiClient = bankApiClient;

    [HttpGet]
    public IActionResult Transfer() => View(new MoneyTransfer());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(MoneyTransfer transfer)
    {
        if (!ModelState.IsValid || transfer.AmountInCents <= 0)
        {
            ViewBag.Error = "Invalid transfer amount";
            return View(transfer);
        }

        if (string.Equals(transfer.FromAccNumber, transfer.ToAccNumber, StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.Error = "cannot transfer to the same account";
            return View(transfer);
        }

        var result = await _bankApiClient.SendAsync<object>(Method.Post, "api/transfers", transfer);
        if (result.Success)
        {
            ViewBag.Message = "Transfer successful";
        }
        else
        {
            ViewBag.Error = result.ErrorMessage ?? "Transfer failed";
        }

        return View(transfer);
    }
}
