using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class UserRowDto
    {
        public int ID { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool Active { get; set; }

        public int AccountCount { get; set; }
    }
}
