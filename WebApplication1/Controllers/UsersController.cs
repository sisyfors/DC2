
namespace WebApplication1.BankAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.BankAPI.Data;
using WebApplication1.BankAPI.Models;

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
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required." });
        }

        var user = new User { Name = request.Name };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.ID }, new
        {
            user.ID,
            user.Name
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
            return NotFound(new { message = "User not found." });
        }

        return Ok(new { user.ID, user.Name });
    }
}

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
}
