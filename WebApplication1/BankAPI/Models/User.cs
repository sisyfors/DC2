namespace WebApplication1.BankAPI.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int ID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
    public string PathToPicture { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string PasswordSalted { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public List<BankAccount> Accounts { get; set; } = new();
}