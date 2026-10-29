namespace WebApplication2.Models
{
    public class MoneyTransfer
    {
        public string FromAccNumber { get; set; } = string.Empty;
        public string ToAccNumber { get; set; } = string.Empty;
        public long AmountInCents { get; set; }
        public string TransferDescription { get; set; } = string.Empty;
    }
}
