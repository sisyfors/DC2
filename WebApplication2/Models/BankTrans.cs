using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class BankTrans
    {
        public int ID { get; set; }

        public int AccountID { get; set; }

        public int? RelatedAccountID { get; set; }

        [Required]
        [MaxLength(20)]
        public string TransactionType { get; set; } = string.Empty;

        public long AmountInCents { get; set; }

        public string TransferDescription { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Guid? TransferID { get; set; }
    }
}
