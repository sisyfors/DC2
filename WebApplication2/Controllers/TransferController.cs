using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class TransferController : Controller
    {
        [HttpGet]
        public IActionResult Transfer()
        {
            return View(new MoneyTransfer());
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(MoneyTransfer transfer)
        {
            if (transfer.AmountInCents <= 0)
            {
                ViewBag.Error = "Invalid transfer amount";
                return View(transfer);
            }

            RestClient restClient = new RestClient("https://localhost:7285/");

            RestRequest restRequest = new RestRequest("api/transfers", Method.Post);

            restRequest.AddJsonBody(transfer);

            RestResponse restResponse = await restClient.ExecuteAsync(restRequest);

            if (restResponse.IsSuccessful)
            {
                ViewBag.Message = "Transfer successful";
            }
            else
            {
                ViewBag.Error = "Transfer failed " + restResponse.Content;
            }

            return View(transfer);
        }
    }
}
