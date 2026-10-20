namespace WebApplication1.BankAPI.Models;

public class ErrorData
{
    public string error { get; set; } = string.Empty;

    public string message { get; set; } = string.Empty;

    public Dictionary<string, string>? details { get; set; }
}