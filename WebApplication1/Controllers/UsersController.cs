
namespace WebApplication1.BankAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.BankAPI.Data;
using WebApplication1.BankAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;

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

        user.UserPasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);

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

    [HttpGet("SearchByEmail")]
    public async Task<IActionResult> SearchByEmail(string? userEmail)
    {
        var details = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            details.Add("UserEmail", "email is required");
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

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(usr => usr.UserEmail == userEmail);

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
            user.ID,
            user.Username,
            user.Name,
            user.UserEmail,
            user.UserAddress,
            user.UserPhone,
            user.UserPfpPath,
            user.UserActiveFlag
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

        var loginResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.UserPasswordHash, request.Password);

        if(loginResult == PasswordVerificationResult.Failed)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "required feilds are incorrect",
                details = null
            });
        }

        if(user.UserActiveFlag == false)
        {
            return BadRequest(new ErrorData
            {
                error = "INVALID_ERROR",
                message = "account is not active",
                details = null
            });
        }

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

        using (var stream = new MemoryStream())
        {
            await picture.CopyToAsync(stream);
            System.IO.File.WriteAllBytes(filePath, stream.ToArray());
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
}