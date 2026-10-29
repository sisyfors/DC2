using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class AuditDto
    {
        public string Admin { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        public DateTime Date { get; set; }
    }
}
