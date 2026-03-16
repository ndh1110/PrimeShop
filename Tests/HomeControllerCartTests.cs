using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using _1298_DUYHUNG.Controllers;
using _1298_DUYHUNG.Data;
using _1298_DUYHUNG.Models;
using _1298_DUYHUNG.Extensions;
using System.Text.Json;
using System.Text;

namespace _1298_DUYHUNG.Tests
{
    public class HomeControllerCartTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly HomeController _controller;
        private readonly Mock<ISession> _mockSession;

        public HomeControllerCartTests()
        {
            // 1. Setup Database ảo (InMemory) giống như Kha làm
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            // 2. Setup Session ảo (Mock Session) vì Giỏ hàng lưu trong Session
            _mockSession = new Mock<ISession>();
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Session = _mockSession.Object;

            _controller = new HomeController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext
                }
            };
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void AddToCart_WithNewProduct_ReturnsRedirectToIndex()
        {
            // Arrange: Tạo 1 sản phẩm ảo trong DB
            var product = new Product { Id = 1, Name = "Bàn phím cơ", Price = 1000000 };
            _context.Products.Add(product);
            _context.SaveChanges();

            // Giả lập Session giỏ hàng đang trống
            byte[] emptySessionValue = null;
            _mockSession.Setup(s => s.TryGetValue("Cart", out emptySessionValue)).Returns(false);

            // Act: Gọi hàm thêm 2 sản phẩm vào giỏ
            var result = _controller.AddToCart(1, 2);

            // Assert: Kiểm tra xem có chuyển hướng về trang chủ thành công không
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public void RemoveFromCart_WhenItemExists_ReturnsSuccessJson()
        {
            // Arrange: Giả lập trong Session đang có sẵn 1 sản phẩm ID = 1
            var cart = new List<CartItem> 
            { 
                new CartItem { ProductId = 1, ProductName = "Bàn phím cơ", Quantity = 2 } 
            };
            var cartJson = JsonSerializer.Serialize(cart);
            var cartBytes = Encoding.UTF8.GetBytes(cartJson);
            
            _mockSession.Setup(s => s.TryGetValue("Cart", out cartBytes)).Returns(true);

            // Act: Gọi hàm xóa sản phẩm ID = 1
            var result = _controller.RemoveFromCart(1) as JsonResult;

            // Assert: Kiểm tra kết quả trả về dạng JSON có success = true không
            Assert.NotNull(result);
            dynamic data = result.Value;
            Assert.True((bool)data.GetType().GetProperty("success").GetValue(data, null));
        }

        [Fact]
        public void GetCartCount_ReturnsTotalQuantityOfItems()
        {
            // Arrange: Giả lập giỏ hàng có 2 món (tổng số lượng là 3+2 = 5)
            var cart = new List<CartItem> 
            { 
                new CartItem { ProductId = 1, Quantity = 3 },
                new CartItem { ProductId = 2, Quantity = 2 }
            };
            var cartJson = JsonSerializer.Serialize(cart);
            var cartBytes = Encoding.UTF8.GetBytes(cartJson);

            _mockSession.Setup(s => s.TryGetValue("Cart", out cartBytes)).Returns(true);

            // Act
            var result = _controller.GetCartCount() as JsonResult;

            // Assert: Kiểm tra xem API trả về count = 5 không
            Assert.NotNull(result);
            dynamic data = result.Value;
            Assert.Equal(5, (int)data.GetType().GetProperty("count").GetValue(data, null));
        }
    }
}