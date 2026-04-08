using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using _1298_DUYHUNG.Models;
using Xunit;

namespace _1298_DUYHUNG.Tests
{
    public class AccountServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;

        public AccountServiceTests()
        {
            // Thiết lập Mock cho UserManager (Yêu cầu bắt buộc của Identity)
            var store = new Mock<IUserStore<User>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var passwordHasher = new Mock<IPasswordHasher<User>>();
            var userValidators = new List<IUserValidator<User>>();
            var passwordValidators = new List<IPasswordValidator<User>>();
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new IdentityErrorDescriber();
            var services = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<User>>>();

            _userManagerMock = new Mock<UserManager<User>>(
                store.Object,
                options.Object,
                passwordHasher.Object,
                userValidators,
                passwordValidators,
                keyNormalizer.Object,
                errors,
                services.Object,
                logger.Object
            );
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccess()
        {
            // Arrange: Giả lập dữ liệu đăng ký hợp lệ
            var model = new RegisterViewModel { UserName = "hung_hutech", Email = "hung@gmail.com", Password = "Password123!" };
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act: Thực hiện hành động đăng ký
            var result = await _userManagerMock.Object.CreateAsync(new User { UserName = model.UserName }, model.Password);

            // Assert: Kiểm tra kết quả trả về phải thành công
            Assert.True(result.Succeeded);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), "Password123!"), Times.Once);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsFailure()
        {
            // Arrange: Giả lập lỗi trùng Email từ hệ thống
            var error = new IdentityError { Description = "Email already exists" };
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(error));

            // Act
            var result = await _userManagerMock.Object.CreateAsync(new User(), "any_pass");

            // Assert: Kiểm tra hệ thống có báo lỗi không
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Description == "Email already exists");
        }
    }
}