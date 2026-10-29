using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.BankAPI.Data;
using WebApplication1.BankAPI.Models;
using Microsoft.Data.Sqlite;
using System.Globalization;


namespace WebApplication1.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly BankDbContext _context;

        public TransactionsController(BankDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions(string? transactionType, string? sortOrder)
        {
            List<BankTrans> transferTransactions = await _context.BankTransactions.ToListAsync();

            if (!string.IsNullOrEmpty(transactionType))
            {
                transferTransactions = transferTransactions.Where(transaction => transaction.TransactionType == transactionType).ToList();
            }

            if (sortOrder == "oldest")
            {
                transferTransactions = transferTransactions.OrderBy(transaction => transaction.CreatedAtUtc).ToList();
            }
            else
            {
                transferTransactions = transferTransactions.OrderByDescending(transaction => transaction.CreatedAtUtc).ToList();
            }

            return Ok(transferTransactions);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchTransactions(string accountNumber, string? transactionType, string? sortOrder)
        {
            List<BankTrans> transactions = await _context.BankTransactions.Where(
                transaction => _context.BankAccounts.Any(
                    account => account.ID == transaction.AccountID &&
                               account.AccountNumber == accountNumber)).ToListAsync();

            if (!string.IsNullOrEmpty(transactionType))
            {
                transactions = transactions.Where(transaction => transaction.TransactionType == transactionType).ToList();
            }

            if (sortOrder == "oldest")
            {
                transactions = transactions.OrderBy(transaction => transaction.CreatedAtUtc).ToList();
            }
            else
            {
                transactions = transactions.OrderByDescending(transaction => transaction.CreatedAtUtc).ToList();
            }

            return Ok(transactions);
        }
    }
}
