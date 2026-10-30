using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using WebApplication1.BankAPI.Data;
using WebApplication1.BankAPI.Models;
using WebApplication1.BankAPI.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly BankDbContext _dbContext;

        public AdminController(BankDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IActionResult Error(
            int status,
            string code,
            string msg,
            object? details = null
            )
        {
            ErrorData error = new ErrorData();

            error.error = code;
            error.message = msg;
            error.details = details as Dictionary<string, string>;

            return StatusCode(status, error);
        }

        [HttpGet("api/users-list")]
        public async Task<IActionResult> List(
            string? search,
            int page = 1,
            int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return Error(400, "INVALID_PAGINATION", "Invalid paging values.");
            }

            var queue = _dbContext.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var searchTrim = search.Trim();

                queue = queue.Where(
                    u => u.Name.Contains(searchTrim) ||
                    u.Username.Contains(searchTrim) ||
                    u.UserEmail.Contains(searchTrim) ||
                    u.Accounts.Any(a => a.AccountNumber.Contains(searchTrim))
                    );

            }

            int total = await queue.CountAsync();
            var items = await queue.OrderBy(u => u.Name)
                .ThenBy(u => u.ID)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new
                {
                    u.ID,
                    u.Name,
                    u.Username,
                    Email = u.UserEmail,
                    u.Role,
                    Active = u.UserActiveFlag,
                    AccountCount = u.Accounts.Count
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount = total,
                totalPages = (int)Math.Ceiling(total / (double)pageSize),
                items
            });
        }

        [HttpGet("api/admin/users/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _dbContext.Users.AsNoTracking()
                .Where(u => u.ID == id)
                .Select(u => new
                {
                    u.ID,
                    u.Name,
                    u.Username,
                    Email = u.UserEmail,
                    Address = u.UserAddress,
                    PhoneNumber = u.UserPhone,
                    u.Role,
                    Active = u.UserActiveFlag,
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Error(404, "USER_NOT_FOUND", $"User {id} not found.");
            }

            return Ok(user);
        }


        [HttpPost("api/admin/users")]
        public async Task<IActionResult> Create(AdminCreateUserRequest request)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                errors["Name"] = "Name is required";
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                errors["Username"] = "Username is required";
            }

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
            {
                errors["Email"] = "Need a valid Email";
            }

            if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
            {
                errors["Password"] = "Password must be at least 8 characters";
            }

            if (request.Role != "Customer" &&  request.Role != "Administrator")
            {
                errors["Role"] = "Role must be Customer or Administrator";
            }

            if (errors.Count > 0)
            {
                return Error(400, "VALIDATION_ERROR", "Invalid user details", errors);
            }

            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                return Error(409, "USERNAME_TAKEN", "Username already taken");
            }

            if (await _dbContext.Users.AnyAsync(u => u.UserEmail == request.Email))
            {
                return Error(409, "EMAIL_TAKEN", "Email already taken");
            }

            PasswordGen.HashPassword(request.Password, out var hash, out var salt);

            User user = new User
            {
                Name = request.Name,
                Username = request.Username,
                UserEmail = request.Email,
                UserAddress = request.Address ?? "",
                UserPhone = request.PhoneNumber ?? "",
                UserPfpPath = "default.png",
                UserActiveFlag = true,
                Role = request.Role,
                UserPasswordHash = hash,
                UserPasswordSalt = salt
            };

            _dbContext.Users.Add(user);

            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = user.ID },
                new { user.ID }
                );

        }

        [HttpPut("api/admin/users/{id:int}")]
        public async Task<IActionResult> Update(int id, AdminUpdateUserRequest request)
        {
            var user = await _dbContext.Users.FindAsync(id);

            if (user == null)
            {
                return Error(404, "USER_NOT_FOUND", $"User {id} not found");
            }

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                !request.Email.Contains("@"))
            {
                return Error(
                    400,
                    "VALIDATION_ERROR",
                    "Invalid updated user details",
                    new Dictionary<string, string>
                    {
                        ["NameOrEmail"] = "Name and a valid email required"
                    });
            }

            if (await _dbContext.Users.AnyAsync(u => u.UserEmail == request.Email && u.ID != id))
            {
                return Error(409, "EMAIL_TAKEN", "Email is already taken");
            }

            user.Name = request.Name;
            user.UserEmail = request.Email;
            user.UserAddress = request.Address ?? "";
            user.UserPhone = request.PhoneNumber ?? "";

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("api/admin/users/{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);

            if (user == null)
            {
                return Error(404, "USER_NOT_FOUND", $"User {id} not found");
            }

            user.UserActiveFlag = false;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("api/admin/users/{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 8)
            {
                return Error(
                    400,
                    "VALIDATION_ERROR",
                    "Invalid new password",
                    new Dictionary<string, string>
                    {
                        ["NewPassword"] = "Password must be at least 8 characters"
                    });
            }

            var user = await _dbContext.Users.FindAsync(id);

            if (user == null)
            {
                return Error(404, "USER_NOT_FOUND", $"User {id} not found");
            }

            PasswordGen.HashPassword(request.NewPassword, out var hash, out var salt);
            user.UserPasswordHash = hash;
            user.UserPasswordSalt = salt;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("api/audit")]
        public async Task<IActionResult> AddAudit(AuditRequest request)
        {
            if (!await _dbContext.Users.AnyAsync(
                u => u.ID == request.AdminUserId &&
                u.Role == "Administrator"))
            {
                return Error(400, "VALIDATION_ERROR", "Audit entries must come from admin");
            }

            string details = request.Details;

            if (details.Length > 500)
            {
                details = details.Substring(0, 500);
            }

            _dbContext.AuditEntries.Add(
                new AuditEntry
                {
                    AdminUserID = request.AdminUserId,
                    Action = request.Action,
                    Details = details
                });

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("api/audit")]
        public async Task<IActionResult> GetAudit(int page = 1, int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return Error(400, "VALIDATION_ERROR", "Invalid paging values");
            }

            var queue = _dbContext.AuditEntries.AsNoTracking();

            int total = await queue.CountAsync();
            var items = await queue.OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.ID)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(a => new
                {
                    a.ID,
                    Admin = a.AdminUser!.Name,
                    a.Action,
                    a.Details,
                    Date = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount = total,
                totalPages = (int)Math.Ceiling(total / (double)pageSize),
                items
            });


        }
    }
}
