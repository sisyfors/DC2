using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class AuditPageDto
    {
        public int Page { get; set; } = 1;

        public int TotalPages { get; set; }

        public List<AuditDto> Items { get; set; } = new List<AuditDto>();
    }
}
