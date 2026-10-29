using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using WebApplication1.BankAPI.Data;
using WebApplication1.BankAPI.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/transfers")]
    public class TransferController : ControllerBase
    {
        private readonly BankDbContext _context;

        public TransferController(BankDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(MoneyTransfer request)
        {
            if (request.AmountInCents <= 0)
                return BadRequest(new { message = "Invalid transfer amount" });

            var fromUserAccount = await _context.BankAccounts.FirstOrDefaultAsync(acct => acct.AccountNumber == request.FromAccNumber);

            var toUserAccount = await _context.BankAccounts.FirstOrDefaultAsync(acct => acct.AccountNumber == request.ToAccNumber);

            if (fromUserAccount == null || toUserAccount == null)
                return NotFound(new { message = "account not found" });

            if (!fromUserAccount.IsActive || !toUserAccount.IsActive)
                return BadRequest(new { message = "account is not active" });

            if (fromUserAccount.BalanceInCents < request.AmountInCents)
                return BadRequest(new { message = "not enough funds in account" });

            // we need to verify user session

            var transferId = Guid.NewGuid();

            fromUserAccount.BalanceInCents = fromUserAccount.BalanceInCents - request.AmountInCents;

            toUserAccount.BalanceInCents = toUserAccount.BalanceInCents + request.AmountInCents;

            _context.BankTransactions.Add(new BankTrans
            {
                AccountID = fromUserAccount.ID,
                RelatedAccountID = toUserAccount.ID,
                TransactionType = "TransferOut",
                AmountInCents = request.AmountInCents,
                TransferDescription = request.TransferDescription,
                TransferID = transferId
            });

            _context.BankTransactions.Add(new BankTrans
            {
                AccountID = toUserAccount.ID,
                RelatedAccountID = fromUserAccount.ID,
                TransactionType = "TransferIn",
                AmountInCents = request.AmountInCents,
                TransferDescription = request.TransferDescription,
                TransferID = transferId
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "transfer success", transferId });
        }
    }
}
