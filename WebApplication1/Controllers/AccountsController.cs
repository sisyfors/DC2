
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
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            return BadRequest(new { message = "Account number is required." });
        }

        if (request.AccountNumber.Length > 20)
        {
            return BadRequest(new { message = "Account number cannot exceed 20 characters." });
        }

        if (request.UserID <= 0)
        {
            return BadRequest(new { message = "A valid user ID is required." });
        }

        if (request.AccountType != "Savings" &&
            request.AccountType != "Checking")
        {
            return BadRequest(new { message = "Account type must be Savings or Checking." });
        }

        if (request.InitialBalanceInCents < 0)
        {
            return BadRequest(new { message = "Initial balance cannot be negative." });
        }

        var userExists = await _context.Users
            .AnyAsync(u => u.ID == request.UserID);

        if (!userExists)
        {
            return NotFound(new { message = "User not found." });
        }

        var accountExists = await _context.BankAccounts
            .AnyAsync(a => a.AccountNumber == request.AccountNumber);

        if (accountExists)
        {
            return Conflict(new { message = "Account number already exists." });
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
            return NotFound(new { message = "Account not found." });
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
