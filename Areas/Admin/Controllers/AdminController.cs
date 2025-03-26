using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using _1298_DUYHUNG.Models; // Namespace của User

namespace _1298_DUYHUNG.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")] // Đảm bảo controller thuộc Area "Admin"
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager; // Sửa thành UserManager<User>

        public AdminController(UserManager<User> userManager) // Sửa constructor
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> AccountManagement()
        {
            // Lấy tài khoản Admin trước
            var adminUser = await _userManager.FindByEmailAsync("admin@example.com");
            var usersList = new List<UserViewModel>();

            // Nếu tài khoản Admin tồn tại, thêm vào đầu danh sách
            if (adminUser != null)
            {
                usersList.Add(new UserViewModel
                {
                    Name = adminUser.UserName,
                    Email = adminUser.Email,
                    PhoneNumber = adminUser.PhoneNumber,
                    Role = string.Join(", ", await _userManager.GetRolesAsync(adminUser))
                });
            }

            // Lấy danh sách các tài khoản còn lại (trừ tài khoản Admin)
            var otherUsers = _userManager.Users
                .Where(u => u.Email != "admin@example.com")
                .Select(u => new UserViewModel
                {
                    Name = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = string.Join(", ", _userManager.GetRolesAsync(u).Result)
                }).ToList();

            // Gộp danh sách: Admin (nếu có) ở đầu, sau đó là các tài khoản khác
            usersList.AddRange(otherUsers);

            return View(usersList);
        }
    }
}