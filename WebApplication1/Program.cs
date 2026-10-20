using WebApplication1.BankAPI.Data;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
