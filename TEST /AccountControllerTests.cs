using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using _1298_DUYHUNG.Controllers;
using _1298_DUYHUNG.Models;
using Xunit;

namespace _1298_DUYHUNG.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;

        public AccountControllerTests()
        {
            var userStore = new Mock<IUserStore<User>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(o => o.Value).Returns(new IdentityOptions());
            var passwordHasher = new Mock<IPasswordHasher<User>>();
            var userValidators = new List<IUserValidator<User>>();
            var passwordValidators = new List<IPasswordValidator<User>>();
            var normalizer = new Mock<ILookupNormalizer>();
            var describer = new IdentityErrorDescriber();
            var services = new Mock<IServiceProvider>();
            var userLogger = new Mock<ILogger<UserManager<User>>>();

            _userManagerMock = new Mock<UserManager<User>>(userStore.Object, options.Object, passwordHasher.Object,
                userValidators, passwordValidators, normalizer.Object, describer, services.Object, userLogger.Object);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            optionsAccessor.Setup(o => o.Value).Returns(new IdentityOptions());
            var logger = new Mock<ILogger<SignInManager<User>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<User>>();

            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                optionsAccessor.Object,
                logger.Object,
                schemes.Object,
                confirmation.Object);
        }

        private AccountController CreateController(bool captchaResult = true)
        {
            return new TestableAccountController(_userManagerMock.Object, _signInManagerMock.Object, captchaResult);
        }

        [Fact]
        public async Task Login_WithValidCredentials_RedirectsHomeAndUpdatesLastLogin()
        {
            // Arrange
            var controller = CreateController(true);
            var user = new User { UserName = "alice" };

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync("alice", "P@ssw0rd", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _userManagerMock.Setup(x => x.FindByNameAsync("alice")).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.Login("alice", "P@ssw0rd", "token");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.NotNull(user.LastLogin);
            Assert.True((DateTime.UtcNow - user.LastLogin!.Value).TotalSeconds < 5);
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
            _signInManagerMock.Verify(x => x.PasswordSignInAsync("alice", "P@ssw0rd", false, false), Times.Once);
        }

        [Fact]
        public async Task Login_WhenCaptchaFails_ReturnsViewWithModelError()
        {
            // Arrange
            var controller = CreateController(false);

            // Act
            var result = await controller.Login("alice", "P@ssw0rd", "bad-token");

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            _signInManagerMock.Verify(
                x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_AddsErrorAndReturnsView()
        {
            // Arrange
            var controller = CreateController(true);

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync("alice", "wrong", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await controller.Login("alice", "wrong", "token");

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState, kvp => kvp.Value.Errors.Any());
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        private class TestableAccountController : AccountController
        {
            private readonly bool _captchaResult;

            public TestableAccountController(UserManager<User> userManager, SignInManager<User> signInManager, bool captchaResult)
                : base(userManager, signInManager)
            {
                _captchaResult = captchaResult;
            }

            protected override Task<bool> VerifyCaptcha(string captchaResponse)
            {
                return Task.FromResult(_captchaResult);
            }
        }
    }
}
