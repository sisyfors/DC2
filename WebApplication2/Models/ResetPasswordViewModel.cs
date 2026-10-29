using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class ResetPasswordViewModel
    {
        public int UserId {  get; set; }
        [Required, MinLength(8), DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
