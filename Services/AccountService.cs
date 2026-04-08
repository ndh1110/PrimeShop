using Microsoft.AspNetCore.Identity;
using _1298_DUYHUNG.Models;

namespace _1298_DUYHUNG.Services
{
    public class AccountService
    {
        private readonly UserManager<User> _userManager;

        public AccountService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
        {
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                DateCreated = DateTime.UtcNow
            };
            return await _userManager.CreateAsync(user, model.Password);
        }
    }
}