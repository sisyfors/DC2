using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class EditUserViewModel
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber {  get; set; } = string.Empty;
    }
}
