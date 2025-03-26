using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using _1298_DUYHUNG.Models;
using _1298_DUYHUNG.Models.ViewModels;

namespace _1298_DUYHUNG.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AccountManagement(string query, int page = 1, int pageSize = 10)
        {
            var users = _userManager.Users.AsQueryable();

            // Đặt tài khoản Admin lên đầu
            var adminUser = await _userManager.FindByEmailAsync("admin@example.com");
            var usersList = new List<(User User, string Role)>();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(query))
            {
                query = query.ToLower();
                users = users.Where(u => (u.UserName != null && u.UserName.ToLower().Contains(query)) || 
                                        (u.Email != null && u.Email.ToLower().Contains(query)));
            }

            // Phân trang, sắp xếp theo Id
            var totalUsers = users.Count();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            users = users
                .OrderBy(u => u.Id) // Thêm OrderBy
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            // Lấy danh sách người dùng và vai trò
            var userList = users.ToList();
            if (adminUser != null && !userList.Contains(adminUser))
            {
                userList.Insert(0, adminUser); // Đặt Admin lên đầu
            }

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersList.Add((user, roles.FirstOrDefault() ?? "None"));
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Query = query;
            ViewBag.PageSize = pageSize;

            return View(usersList);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction("AccountManagement");
            }
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Role = roles.FirstOrDefault() ?? "None";
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction("AccountManagement");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Address = user.Address ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "None"
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction("AccountManagement");
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // Cập nhật vai trò
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            if (!string.IsNullOrEmpty(model.Role) && model.Role != "None")
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            TempData["Success"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction("AccountManagement");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction("AccountManagement");
            }

            // Không cho phép xóa tài khoản Admin
            if (user.Email == "admin@example.com")
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin.";
                return RedirectToAction("AccountManagement");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Xóa tài khoản thành công.";
            }
            else
            {
                TempData["Error"] = "Xóa tài khoản thất bại.";
            }
            return RedirectToAction("AccountManagement");
        }
    }
}