
namespace WebApplication1.BankAPI.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.EntityFrameworkCore;
using WebApplication1.BankAPI.Data;
using WebApplication1.BankAPI.Models;
using WebApplication1.BankAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly BankDbContext _context;

    public UsersController(BankDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var details = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            details.Add("Username", "username is a required input");
        }
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            details.Add("Name", "name is a required input");
        }
        if (string.IsNullOrWhiteSpace(request.UsrEmail))
        {
            details.Add("UsrEmail", "email is a required input");
        }
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            details.Add("Password", "password is a required input");
        }
        if (details.Count > 0)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "required fields are missing",
                details = details
            });
        }
        // Check if request is a duplicate
        var existingUserFlag = await _context.Users.FirstOrDefaultAsync(user => user.Username == request.Username ||
            user.UserEmail == request.UsrEmail);

        if(existingUserFlag != null)
        {
            return Conflict(new ErrorData
            {
                error = "USER_EXIST",
                message = "Username or email exists",
                details = null
            });
        }


        var user = new User
        {
            Username = request.Username,
            Name = request.Name,
            UserEmail = request.UsrEmail,
            UserAddress = request.UsrAddress,
            UserPhone = request.UsrPhone
        };

        PasswordGen.HashPassword(request.Password, out string hash, out string salt);

        user.UserPasswordHash = hash;
        user.UserPasswordSalt = salt;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.ID }, new
        {
            user.ID, user.Username,
            user.Name, user.UserEmail,
            user.UserAddress, user.UserPhone,
            user.UserPfpPath, user.UserActiveFlag
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.ID == id);

        if (user == null)
        {
            return NotFound(new ErrorData
            {
                error = "USER_NOT_FOUND",
                message = "user could not be found.",
                details = null
            });
        }

        return Ok(new
        {
            user.ID, user.Username,
            user.Name, user.UserEmail,
            user.UserAddress, user.UserPhone,
            user.UserPfpPath, user.UserActiveFlag
        });
    }

    [HttpGet("Search")]
    public async Task<IActionResult> Search(string? username)
    {
        var details = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(username))
        {
            details.Add("Username", "username is required");
        }

        if (details.Count > 0)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "required fields missing",
                details = details
            });  
        }

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(usr=> usr.Username == username);

        if (user == null)
        {
            return NotFound(new ErrorData
            {
                error = "USER_NOT_FOUND",
                message = "User not found",
                details = null
            });
        }

        return Ok(new
        {
            user.ID, user.Username,
            user.Name, user.UserEmail,
            user.UserAddress, user.UserPhone,
            user.UserPfpPath, user.UserActiveFlag
        });

    }

    [HttpGet("SearchByAccountNumber")]
    public async Task<IActionResult> SearchByAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return BadRequest(new { message = "account number is required" });
        }
        var account = await _context.BankAccounts
            .Include(acc => acc.User)
            .FirstOrDefaultAsync(acc => acc.AccountNumber == accountNumber);

        if (account == null || account.User == null)
        {
            return NotFound(new { message = "user not found" });
        }
        return Ok(new
        {
            account.User.ID,
            account.User.Username,
            account.User.Name,
            account.User.UserEmail,
            account.User.UserAddress,
            account.User.UserPhone,
            account.User.UserActiveFlag
        });
    }

    [HttpPut("{username}/deactivate")]
    public async Task<IActionResult> DeactivateUser(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(user => user.Username == username);

        if (user == null)
        {
            return NotFound(new ErrorData
            {
                error = "USER_NOT_FOUND",
                message = "User not found",
                details = null
            });
        }

        user.UserActiveFlag = false;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.ID,
            user.Username,
            user.UserActiveFlag
        });
    }

    [HttpPost("loginUser")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var details = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            details.Add("Username", "username is required");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            details.Add("Password", "password is required");
        }

        if (details.Count > 0)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "required feilds are missing",
                details = details
            });
        }

        var user = await _context.Users.FirstOrDefaultAsync(user => user.Username == request.Username);

        if (user == null)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "incorrect fields",
                details = null
            });
        }

        var loginResult = PasswordGen.VerifyPassword(request.Password, user.UserPasswordHash, user.UserPasswordSalt);

        if (loginResult == false)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "incorrect username or password",
                details = null
            });
        }

        if (user.UserActiveFlag == false)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "account is not active",
                details = null
            });
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Ok(new
        {
            user.ID,
            user.Username
        });
    }

    [HttpPut("{username}")]
    public async Task<IActionResult> UpdateUser(string username, UpdateUserRequest request)
    {

        var details = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            details.Add("Name", "name is required");
        }

        if (string.IsNullOrWhiteSpace(request.UserEmail))
        {
            details.Add("UserEmail", "email is required");
        }

        if (details.Count > 0)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "required feilds are missing",
                details = details
            });
        }

        var user = await _context.Users.FirstOrDefaultAsync(user => user.Username == username);

        if (user == null)
        {
            return NotFound(new ErrorData
            {
                error = "USER_NOT_FOUND",
                message = "User not found",
                details = null
            });
        }

        var existingEmail = await _context.Users.FirstOrDefaultAsync(usr => usr.UserEmail == request.UserEmail && usr.ID != user.ID);

        if (existingEmail != null)
        {
            return Conflict(new ErrorData
            {
                error = "EMAIL_EXIST",
                message = "email exists",
                details = null
            });
        }

        user.Name = request.Name;
        user.UserEmail = request.UserEmail;
        user.UserAddress = request.UserAddress;
        user.UserPhone = request.UserPhone;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.ID,
            user.Username,
            user.Name,
            user.UserEmail,
            user.UserAddress,
            user.UserPhone
        });
    }

    [HttpPut("{username}/pfp")]
    public async Task<IActionResult> UploadProfilePicture(string username, IFormFile? picture)
    {
        var user = await _context.Users.FirstOrDefaultAsync(usr => usr.Username == username);

        if (user == null)
        {
            return NotFound(new ErrorData
            {
                error = "USER_NOT_FOUND",
                message = "user not found",
                details = null
            });
        }

        if (picture == null)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "Picture is required",
                details = new Dictionary<string, string>
            {
                { "picture", "picture is required" }
            }
            });
        }

        if (picture.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "file must be 10 MB or smaller",
                details = null
            });
        }

        var extension = Path.GetExtension(picture.FileName).ToLowerInvariant();
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "file not allowed",
                details = null
            });
        }

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Pfp");
        Directory.CreateDirectory(folderPath);

        var fileName = user.ID.ToString() + extension;
        var filePath = Path.Combine(folderPath, fileName);

        var file = System.IO.File.Create(filePath);

        try
        {
            await picture.CopyToAsync(file);
        }
        finally
        {
            file.Close();
        }

        user.UserPfpPath = Path.Combine("Pfp", fileName);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.ID,
            user.Username,
            user.UserPfpPath
        });
    }

    [HttpGet("{username}/pfp")]
    public async Task<IActionResult> GetProfilePicture(string username)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(usr => usr.Username == username);

        if (user == null)
        {
            return NotFound(new ErrorData
            {
                error = "USER_NOT_FOUND",
                message = "user not found",
                details = null
            });
        }

        if (string.IsNullOrWhiteSpace(user.UserPfpPath))
        {
            return NotFound(new ErrorData
            {
                error = "PICTURE_NOT_FOUND",
                message = "no profile picture found for user",
                details = null
            });
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), user.UserPfpPath);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new ErrorData
            {
                error = "PICTURE_NOT_FOUND",
                message = "Profile picture file not found",
                details = null
            });
        }

        var fileType = new Dictionary<string, string>
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" }
        };


        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        var contentType = fileType[extension];

        var pfpBytes = System.IO.File.ReadAllBytes(filePath);

        return File(pfpBytes, contentType);
    }

    [HttpPut("{username}/password")]
    public async Task<IActionResult> ChangePassword(string username, PasswordChangeRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(user => user.Username == username);

        if (user == null)
            return NotFound(new { message = "sser not found." });

        if (string.IsNullOrWhiteSpace(request.NewUserPassword))
            return BadRequest(new { message = "new password is required" });

        bool validPassword = PasswordGen.VerifyPassword(
            request.OldUserPassword,
            user.UserPasswordHash,
            user.UserPasswordSalt);

        if (!validPassword)
            return BadRequest(new { message = "current password is incorrect" });

        PasswordGen.HashPassword(
            request.NewUserPassword,
            out string hash,
            out string salt);

        user.UserPasswordHash = hash;
        user.UserPasswordSalt = salt;

        await _context.SaveChangesAsync();

        return Ok(new { message = "password updated successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(user => new
            {
                user.ID,
                user.Username,
                user.Name,
                user.UserEmail,
                user.UserAddress,
                user.UserPhone,
                user.UserActiveFlag
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPut("{username}/reset-password")]
    public async Task<IActionResult> ResetPassword(string username, ResetPasswordRequest resetRequest)
    {
        var user = await _context.Users.FirstOrDefaultAsync(user => user.Username == username);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }
        if (string.IsNullOrWhiteSpace(resetRequest.NewPassword))
        {
            return BadRequest(new { message = "Password is required" });
        }
        PasswordGen.HashPassword(resetRequest.NewPassword, out string passwordHash, out string passwordSalt);

        user.UserPasswordHash = passwordHash;
        user.UserPasswordSalt = passwordSalt;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Password reset successfully" });
    }
}