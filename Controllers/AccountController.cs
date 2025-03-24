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
        public async Task<IActionResult> Register(string username, string email, string password, string hcaptchaResponse)
        {
            if (!await VerifyCaptcha(hcaptchaResponse))
            {
                ModelState.AddModelError("", "Captcha không hợp lệ.");
                return View();
            }

            var user = new User { UserName = username, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Guest");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ
        }

        private async Task<bool> VerifyCaptcha(string captchaResponse)
        {
            // Bỏ qua xác thực hCaptcha, luôn trả về true
            return true;

            // Code gốc (đã được comment để tham khảo):
            /*
            var secretKey = "ES_da0b40c15db24731a974a4666e2a065f";
            var client = new HttpClient();
            var response = await client.PostAsync(
                "https://hcaptcha.com/siteverify",
                new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secretKey),
                    new KeyValuePair<string, string>("response", captchaResponse)
                })
            );
            var result = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(result);
            return (bool)json["success"];
            */
        }
    }
}