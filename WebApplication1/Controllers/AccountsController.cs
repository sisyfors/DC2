
namespace WebApplication1.BankAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.BankAPI.Data;
using WebApplication1.BankAPI.Models;
using Microsoft.AspNetCore.Authorization;


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

    [HttpPut("{accountNumber}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateAccount(
        string accountNumber,
        [FromBody] UpdateAccountRequest request)
    {
        if (request.AccountType != "Savings" && request.AccountType != "Checking")
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ACCOUNT_TYPE",
                message = "Account type must be Savings or Checking",
                details = null
            });
        }

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        if (account == null)
        {
            return NotFound(new ErrorData
            {
                error = "ACCOUNT_NOT_FOUND",
                message = "Account not found",
                details = null
            });
        }

        account.AccountType = request.AccountType;
        await _context.SaveChangesAsync();

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

    [HttpPost("{accountNumber}/deactivate")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeactivateAccount(string accountNumber)
    {
        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        if (account == null)
        {
            return NotFound(new ErrorData
            {
                error = "ACCOUNT_NOT_FOUND",
                message = "Account not found",
                details = null
            });
        }

        if (!account.IsActive)
        {
            return BadRequest(new ErrorData
            {
                error = "ACCOUNT_INACTIVE",
                message = "Account is already inactive",
                details = null
            });
        }

        account.IsActive = false;
        await _context.SaveChangesAsync();

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

    [HttpPost("{accountNumber}/deposit")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Deposit(
        string accountNumber,
        [FromBody] AccountTransactionRequest request)
    {
        if (request.AmountInCents <= 0)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_AMOUNT",
                message = "Amount must be greater than zero",
                details = null
            });
        }

        var account = await _context.BankAccounts
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

        if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId) ||
            account.UserID != userId)
        {
            return Forbid();
        }

        if (!account.IsActive)
        {
            return BadRequest(new ErrorData
            {
                error = "ACCOUNT_INACTIVE",
                message = "account is not active",
                details = null
            });
        }

        var transaction = new BankTrans
        {
            AccountID = account.ID,
            TransactionType = TransactionTypes.Deposit,
            AmountInCents = request.AmountInCents,
            TransferDescription = request.Description
        };

        account.BalanceInCents += request.AmountInCents;
        _context.BankTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "deposit success",
            transactionId = transaction.ID,
            account.AccountNumber,
            account.BalanceInCents
        });
    }

    [HttpPost("{accountNumber}/withdraw")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Withdraw(
        string accountNumber,
        [FromBody] AccountTransactionRequest request)
    {
        if (request.AmountInCents <= 0)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_AMOUNT",
                message = "Amount must be greater than zero",
                details = null
            });
        }

        var account = await _context.BankAccounts
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

        if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId) ||
            account.UserID != userId)
        {
            return Forbid();
        }

        if (!account.IsActive)
        {
            return BadRequest(new ErrorData
            {
                error = "ACCOUNT_INACTIVE",
                message = "account is not active",
                details = null
            });
        }

        if (account.BalanceInCents < request.AmountInCents)
        {
            return BadRequest(new ErrorData
            {
                error = "INSUFFICIENT_FUNDS",
                message = "not enough funds in account",
                details = null
            });
        }

        var transaction = new BankTrans
        {
            AccountID = account.ID,
            TransactionType = TransactionTypes.Withdrawal,
            AmountInCents = request.AmountInCents,
            TransferDescription = request.Description
        };

        account.BalanceInCents -= request.AmountInCents;
        _context.BankTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "withdrawal success",
            transactionId = transaction.ID,
            account.AccountNumber,
            account.BalanceInCents
        });
    }

    // GET: /api/accounts/{accountNumber}
    [HttpGet("{accountNumber}")]
    [Authorize(Roles = "Customer")]
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

        if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var currentUserId) ||
            account.UserID != currentUserId)
        {
            return Forbid();
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

    [HttpGet("~/api/users/{userId:int}/accounts")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetAccountSummary(int userId)
    {
        if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var currentUserId) ||
            currentUserId != userId)
        {
            return Forbid();
        }

        if (!await _context.Users.AnyAsync(u => u.ID == userId))
        {
            return NotFound(new ErrorData
            {
                error = "USER_NOT_FOUND",
                message = "User not found",
                details = null
            });
        }

        var accounts = await _context.BankAccounts
            .AsNoTracking()
            .Where(a => a.UserID == userId)
            .OrderBy(a => a.AccountNumber)
            .Select(a => new
            {
                a.AccountNumber,
                a.AccountType,
                a.BalanceInCents,
                a.IsActive
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("{userId:int}/history")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetTransactionHistory(
        int userId,
        [FromQuery] string? accountNumber,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string sortBy = "date",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var details = new Dictionary<string, string>();

        if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var currentUserId) ||
            currentUserId != userId)
        {
            return Forbid();
        }

        if (!await _context.Users.AnyAsync(u => u.ID == userId))
        {
            return NotFound(new ErrorData
            {
                error = "USER_NOT_FOUND",
                message = "User not found",
                details = null
            });
        }

        if (page <= 0 || pageSize <= 0)
        {
            details.Add("pagination", "page and pageSize must be positive integers");
            return BadRequest(new ErrorData
            {
                error = "INVALID_PAGINATION",
                message = "Invalid pagination parameters",
                details = details
            });
        }

        if (pageSize > 100)
        {
            details.Add("pageSize", "pageSize cannot exceed 100");
            return BadRequest(new ErrorData
            {
                error = "INVALID_PAGINATION",
                message = "Invalid pagination parameters",
                details = details
            });
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            details.Add("dateRange", "startDate cannot be after endDate");
            return BadRequest(new ErrorData
            {
                error = "INVALID_DATE_RANGE",
                message = "Invalid date range",
                details = details
            });
        }

        if (!new[] { "date", "amount", "type" }.Contains(sortBy.ToLower()))
        {
            details.Add("sortBy", "sortBy must be 'date', 'amount', or 'type'");
            return BadRequest(new ErrorData
            {
                error = "INVALID_SORT",
                message = "Invalid sort parameter",
                details = details
            });
        }

        if (!new[] { "asc", "desc" }.Contains(sortOrder.ToLower()))
        {
            details.Add("sortOrder", "sortOrder must be 'asc' or 'desc'");
            return BadRequest(new ErrorData
            {
                error = "INVALID_SORT",
                message = "Invalid sort parameter",
                details = details
            });
        }

        var accounts = await _context.BankAccounts
            .AsNoTracking()
            .Where(a => a.UserID == userId)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync();

        if (!accounts.Any())
        {
            return Ok(new
            {
                summary = new List<object>(),
                transactions = new List<object>(),
                pagination = new
                {
                    page,
                    pageSize,
                    totalItems = 0,
                    totalPages = 0
                }
            });
        }

        if (!string.IsNullOrWhiteSpace(accountNumber) && !accounts.Any(a => a.AccountNumber == accountNumber))
        {
            return NotFound(new ErrorData
            {
                error = "ACCOUNT_NOT_FOUND",
                message = "Account not found for this user",
                details = null
            });
        }

        var accountIds = accounts
            .Where(a => string.IsNullOrWhiteSpace(accountNumber) || a.AccountNumber == accountNumber)
            .Select(a => a.ID).ToList();

        var transactionQuery = _context.BankTransactions
            .AsNoTracking()
            .Where(t => accountIds.Contains(t.AccountID));

        if (startDate.HasValue)
        {
            transactionQuery = transactionQuery.Where(t => t.CreatedAtUtc >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var endDateInclusive = endDate.Value.Date.AddDays(1);
            transactionQuery = transactionQuery.Where(t => t.CreatedAtUtc < endDateInclusive);
        }

        transactionQuery = sortOrder.ToLower() == "asc"
            ? sortBy.ToLower() switch
            {
                "date" => transactionQuery.OrderBy(t => t.CreatedAtUtc),
                "amount" => transactionQuery.OrderBy(t => t.AmountInCents),
                "type" => transactionQuery.OrderBy(t => t.TransactionType),
                _ => transactionQuery.OrderByDescending(t => t.CreatedAtUtc)
            }
            : sortBy.ToLower() switch
            {
                "date" => transactionQuery.OrderByDescending(t => t.CreatedAtUtc),
                "amount" => transactionQuery.OrderByDescending(t => t.AmountInCents),
                "type" => transactionQuery.OrderByDescending(t => t.TransactionType),
                _ => transactionQuery.OrderByDescending(t => t.CreatedAtUtc)
            };

        var totalItems = await transactionQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var transactions = await transactionQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.ID,
                AccountNumber = t.Account!.AccountNumber,
                t.TransactionType,
                t.AmountInCents,
                t.CreatedAtUtc,
                t.RelatedAccountID
            })
            .ToListAsync();

        var accountsSummary = accounts.Select(a => new
        {
            a.AccountNumber,
            a.AccountType,
            a.BalanceInCents,
            a.IsActive
        }).ToList();

        return Ok(new
        {
            summary = accountsSummary,
            transactions,
            pagination = new
            {
                page,
                pageSize,
                totalItems,
                totalPages
            }
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

public class UpdateAccountRequest
{
    public string AccountType { get; set; } = string.Empty;
}

public class AccountTransactionRequest
{
    public long AmountInCents { get; set; }
    public string Description { get; set; } = string.Empty;
}
