
namespace WebApplication1.BankAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.BankAPI.Data;
using WebApplication1.BankAPI.Models;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly BankDbContext _context;

    public AccountsController(BankDbContext context)
    {
        _context = context;
    }

    // POST: /api/accounts
    [HttpPost]
    public async Task<IActionResult> CreateAccount(
        [FromBody] CreateAccountRequest request)
    {
        var details = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            details.Add("AccountNumber", "Account number is required");
        }

        else if (request.AccountNumber.Length > 20)
        {
            details.Add("AccountNumber", "Account number cannot exceed 20 characters");
        }

        if (request.UserID <= 0)
        {
            details.Add("UserID", "A valid user ID is required");
        }

        if (request.AccountType != "Savings" &&
            request.AccountType != "Checking")
        {
            details.Add("AccountType", "Account type must be Savings or Checking");

        }

        if (request.InitialBalanceInCents < 0)
        {
            details.Add("InitialBalanceInCents", "Initial balance cannot be negative");
        }

        if (details.Count > 0)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "Invalid account details",
                details = details
            });
        }

        var userExists = await _context.Users
            .AnyAsync(u => u.ID == request.UserID);

        if (!userExists)
        {
            return NotFound(new ErrorData
            {
                error = "USER_NOT_FOUND",
                message = "User not found",
                details = null
            });
        }

        var accountExists = await _context.BankAccounts
            .AnyAsync(a => a.AccountNumber == request.AccountNumber);

        if (accountExists)
        {
            return Conflict(new ErrorData
            {
                error = "ACCOUNT_EXISTS",
                message = "account exists",
                details = null
            });
        }

        var account = new BankAccount
        {
            AccountNumber = request.AccountNumber,
            AccountType = request.AccountType,
            BalanceInCents = request.InitialBalanceInCents,
            UserID = request.UserID,
            IsActive = true
        };

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAccount),
            new { accountNumber = account.AccountNumber },
            new
            {
                account.ID,
                account.AccountNumber,
                account.AccountType,
                account.BalanceInCents,
                account.IsActive,
                account.UserID
            });
    }

    // GET: /api/accounts/{accountNumber}
    [HttpGet("{accountNumber}")]
    public async Task<IActionResult> GetAccount(string accountNumber)
    {
        var account = await _context.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        if (account == null)
        {
            return NotFound(new ErrorData
            {
                error = "ACCOUNT_NOT_FOUND",
                message = "account not found",
                details = null
            });
        }

        return Ok(new
        {
            account.ID,
            account.AccountNumber,
            account.AccountType,
            account.BalanceInCents,
            account.IsActive,
            account.UserID
        });
    }
}

public class CreateAccountRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = "Savings";
    public long InitialBalanceInCents { get; set; }
    public int UserID { get; set; }
}
