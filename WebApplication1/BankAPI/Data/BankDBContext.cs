
namespace WebApplication1.BankAPI.Data;

using WebApplication1.BankAPI.Models;
using Microsoft.EntityFrameworkCore;

public class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankTrans> BankTransactions => Set<BankTrans>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique account
        modelBuilder.Entity<BankAccount>()
            .HasIndex(a => a.AccountNumber)
            .IsUnique();

        modelBuilder.Entity<BankAccount>()
            .ToTable(t => t.HasCheckConstraint("CK_BankAccount_Balance", "BalanceInCents >= 0"));

        // unique username
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Username)
            .IsUnique();

        // unique email
        modelBuilder.Entity<User>()
            .HasIndex(user => user.UserEmail)
            .IsUnique();

        // One user can have many bank accounts.
        modelBuilder.Entity<User>()
            .HasMany(u => u.Accounts)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserEmail).IsUnique();

        // Transactions
        modelBuilder.Entity<BankTrans>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BankTrans>()
            .ToTable(t => t.HasCheckConstraint("CK_BankTrans_Amount", "AmountInCents > 0"));

        // Audit
        modelBuilder.Entity<AuditEntry>()
            .HasOne(a => a.AdminUser).WithMany()
            .HasForeignKey(a => a.AdminUserID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
