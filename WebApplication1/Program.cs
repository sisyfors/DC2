using WebApplication1.BankAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using WebApplication1.BankAPI.Models;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
var sharedKeyDirectory = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "SharedAuthKeys"));
Directory.CreateDirectory(sharedKeyDirectory);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(sharedKeyDirectory))
    .SetApplicationName("DC2.SharedAuth");
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".DC2.Auth";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var details = new Dictionary<string, string>();

        foreach (var errorRecord in context.ModelState)
        {
            foreach (var error in errorRecord.Value!.Errors)
            {
                details[errorRecord.Key] = error.ErrorMessage;
            }
        }

        return new BadRequestObjectResult(new ErrorData
        {
            error = "INVALID_ERROR",
            message = "invalid request",
            details = details.Count > 0 ? details : null
        });
    };
});

builder.Services.AddOpenApi();

// Register the SQLite database.
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("BankDb")));


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BankDbContext>();
    dbContext.Database.Migrate();
    DBSeeder.Seed(dbContext);
    Console.WriteLine($"DB file: {Path.GetFullPath(dbContext.Database.GetDbConnection().DataSource)}");
    Console.WriteLine($"Users: {dbContext.Users.Count()}, Accounts: {dbContext.BankAccounts.Count()}, Transactions: {dbContext.BankTransactions.Count()}");
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ErrorData
        {
            error = "INTERNAL_SERVER_ERROR",
            message = "An unexpected server error occurred",
            details = null
        });
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
