namespace WebApplication1.BankAPI.Models;

using System.ComponentModel.DataAnnotations;

public class BankTrans
{
    public int ID { get; set; }

    public int AccountID { get; set; }

    public int? RelatedAccountID { get; set; }

    [Required]
    [MaxLength(20)]
    public string TransactionType { get; set; } = string.Empty;

    public long AmountInCents { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? TransferID { get; set; }

    public BankAccount? Account { get; set; }
}