using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class User
    {
        public int ID { get; set; }

        public string UserName { get; set; }

        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        public string UserAddress { get; set; } = string.Empty;

        public string UserPhone { get; set; } = string.Empty;

        public string UserPfpPath { get; set; } = string.Empty;

    }
}
