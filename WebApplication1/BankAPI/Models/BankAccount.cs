namespace WebApplication1.BankAPI.Models;

using System.ComponentModel.DataAnnotations;

public class BankAccount
{
    public int ID { get; set; }

    [Required]
    [MaxLength(20)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string AccountType { get; set; } = "Savings";

    public long BalanceInCents { get; set; }

    public bool IsActive { get; set; } = true;

    public int UserID { get; set; }

    public User? User { get; set; }

    public List<BankTrans> Transactions { get; set; } = new();
}