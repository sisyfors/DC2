namespace WebApplication1.BankAPI.Models
{
    public class PasswordChangeRequest
    {
        public string OldUserPassword { get; set; } = string.Empty;
        public string NewUserPassword { get; set; } = string.Empty;
    }
}