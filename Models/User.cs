using Microsoft.AspNetCore.Identity;

namespace _1298_DUYHUNG.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty; 
        public DateTime DateCreated { get; set; } = DateTime.UtcNow; // Thêm thuộc tính này
        public DateTime? LastLogin { get; set; } // Thêm thuộc tính này
    }
}