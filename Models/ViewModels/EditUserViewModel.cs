namespace _1298_DUYHUNG.Models.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty; 
        public string PhoneNumber { get; set; } = string.Empty; 
        public string Role { get; set; } = string.Empty;
        public List<string> AvailableRoles { get; set; } = new List<string> { "Admin", "Employee", "Company", "Customer" };
    }
}