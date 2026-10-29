namespace WebApplication1.BankAPI.Models
{
    public class AdminCreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UsrEmail { get; set; } = string.Empty;
        public string UsrAddress { get; set; } = string.Empty;
        public string UsrPhone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role {  get; set; } = "Customer";
    }

}
