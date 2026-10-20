namespace WebApplication1.BankAPI.Models
{
    public class UpdateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserAddress { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
    }
}
