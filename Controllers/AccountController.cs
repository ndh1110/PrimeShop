using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using _1298_DUYHUNG.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace _1298_DUYHUNG.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, string hcaptchaResponse)
        {
            if (!await VerifyCaptcha(hcaptchaResponse))
            {
                ModelState.AddModelError("", "Captcha không hợp lệ.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    user.LastLogin = DateTime.UtcNow; // Cập nhật thời gian đăng nhập cuối
                    await _userManager.UpdateAsync(user);
                }
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model, string hcaptchaResponse)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    DateCreated = DateTime.UtcNow // Thêm DateCreated
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private async Task<bool> VerifyCaptcha(string captchaResponse)
        {
            using var client = new HttpClient();
            var response = await client.PostAsync("https://hcaptcha.com/siteverify", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", "your-secret-key"),
                new KeyValuePair<string, string>("response", captchaResponse)
            }));
            var result = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(result);
            return true;
        }
    }
}