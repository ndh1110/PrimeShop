using Microsoft.AspNetCore.Mvc;
using _1298_DUYHUNG.Data;
using _1298_DUYHUNG.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using BCrypt.Net;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string hcaptchaResponse)
    {
        if (!await VerifyCaptcha(hcaptchaResponse))
        {
            ModelState.AddModelError("", "Captcha không hợp lệ.");
            return View();
        }

        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            HttpContext.Session.SetString("User", user.Username);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu.");
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }


    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string username, string email, string password, string hcaptchaResponse)
    {
        if (!await VerifyCaptcha(hcaptchaResponse))
        {
            ModelState.AddModelError("", "Captcha không hợp lệ.");
            return View();
        }

        if (_context.Users.Any(u => u.Username == username))
        {
            ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
            return View();
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("Login");
    }

    private async Task<bool> VerifyCaptcha(string captchaResponse)
    {
        var secretKey = "ES_da0b40c15db24731a974a4666e2a065f"; // Thay thế bằng Secret Key từ hCaptcha
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
    }
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

}
