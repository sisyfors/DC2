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
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;

    public string UserAddress { get; set; } = string.Empty;

    public string UserPhone { get; set; } = string.Empty;

    public string UserPfpPath { get; set; } = string.Empty;

    public string UserPasswordHash { get; set; } = string.Empty;

    public bool UserActiveFlag { get; set; } = true;

    public List<BankAccount> Accounts { get; set; } = new();
}