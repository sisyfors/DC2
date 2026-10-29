using System.ComponentModel.DataAnnotations;

namespace WebApplication1.BankAPI.Models
{
    public class AuditEntry
    {
        public int ID { get; set; }
        public int AdminUserID { get; set; }
        public User? AdminUser { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
