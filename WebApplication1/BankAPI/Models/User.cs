namespace WebApplication1.BankAPI.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int ID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public List<BankAccount> Accounts { get; set; } = new();
}