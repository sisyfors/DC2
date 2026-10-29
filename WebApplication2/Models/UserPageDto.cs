using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class UserPageDto
    {
        public int Page { get; set; } = 1;

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }

        public List<UserRowDto> Items { get; set; } = new List<UserRowDto>();
    }
}
