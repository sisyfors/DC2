namespace WebApplication1.BankAPI.Models
{
    public class AuditRequest
    {
        public int AdminUserId { get; set; }
        public string Action {  get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

}
