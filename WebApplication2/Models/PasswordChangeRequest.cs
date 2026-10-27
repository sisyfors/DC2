namespace WebApplication2.Models
{
    public class PasswordChangeRequest
    {
        public string CurrentUserPassword { get; set; } = string.Empty;
        public string NewUserPassword { get; set; } = string.Empty;
    }
}
