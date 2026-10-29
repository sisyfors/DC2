namespace WebApplication2.Services
{
    public class ApiError
    {
        public string? Error { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, string>? Details { get; set; }
    }
}
