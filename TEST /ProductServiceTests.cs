using Microsoft.EntityFrameworkCore;
using _1298_DUYHUNG.Data;
using _1298_DUYHUNG.Models;
using _1298_DUYHUNG.Services;
using Xunit;

namespace _1298_DUYHUNG.Tests
{

    public class ProductServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            // Tạo InMemory Database cho mỗi test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _productService = new ProductService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region Test Thêm Sản Phẩm (Add Product)

        [Fact]
        public async Task AddProduct_WithValidProduct_ReturnsSuccess()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Electronics" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Laptop Dell",
                Price = 15000000,
                Description = "Laptop gaming",
                CategoryId = 1,
                ImageUrl = "/images/laptop.jpg"
            };

            // Act
            var result = await _productService.AddProduct(product);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Laptop Dell", result.Name);
            Assert.Equal(15000000, result.Price);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task AddProduct_WithInvalidCategoryId_ThrowsException()
        {
            // Arrange
            var product = new Product
            {
                Name = "Laptop Dell",
                Price = 15000000,
                Description = "Laptop gaming",
                CategoryId = 999, // Category không tồn tại
                ImageUrl = "/images/laptop.jpg"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _productService.AddProduct(product));
        }

        [Fact]
        public async Task AddProduct_WithZeroCategoryId_Success()
        {
            // Arrange
            var product = new Product
            {
                Name = "Laptop Dell",
                Price = 15000000,
                Description = "Laptop gaming",
                CategoryId = 0, // Không thuộc danh mục nào
                ImageUrl = "/images/laptop.jpg"
            };

            // Act
            var result = await _productService.AddProduct(product);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.CategoryId);
            Assert.Equal("Laptop Dell", result.Name);
        }

        #endregion

        #region Test Lấy Danh Sách Sản Phẩm (Get Products List)

        [Fact]
        public async Task GetAllProducts_WhenProductsExist_ReturnsProductList()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Electronics" };
            _context.Categories.Add(category);
            
            _context.Products.AddRange(
                new Product { Name = "Product 1", Price = 1000000, CategoryId = 1 },
                new Product { Name = "Product 2", Price = 2000000, CategoryId = 1 },
                new Product { Name = "Product 3", Price = 3000000, CategoryId = 1 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetAllProducts();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllProducts_WhenNoProducts_ReturnsEmptyList()
        {
            // Act
            var result = await _productService.GetAllProducts();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProducts_WithCategory_ReturnsProductsWithCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Electronics" };
            _context.Categories.Add(category);
            
            _context.Products.Add(new Product 
            { 
                Name = "Laptop", 
                Price = 15000000, 
                CategoryId = 1 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetAllProducts();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.NotNull(result[0].Category);
            Assert.Equal("Electronics", result[0].Category.Name);
        }

        #endregion

        #region Test Tính Giá Sản Phẩm (Calculate Price)

        [Fact]
        public void CalculateTotalPrice_WithValidQuantity_ReturnsCorrectTotal()
        {
            // Arrange
            var product = new Product
            {
                Name = "Laptop Dell",
                Price = 10000000
            };
            int quantity = 3;

            // Act
            var result = _productService.CalculateTotalPrice(product, quantity);

            // Assert
            Assert.Equal(30000000, result);
        }

        [Fact]
        public void CalculateTotalPrice_WithZeroQuantity_ReturnsZero()
        {
            // Arrange
            var product = new Product
            {
                Name = "Laptop Dell",
                Price = 10000000
            };
            int quantity = 0;

            // Act
            var result = _productService.CalculateTotalPrice(product, quantity);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateTotalPrice_WithOneQuantity_ReturnsPrice()
        {
            // Arrange
            var product = new Product
            {
                Name = "Mouse",
                Price = 500000
            };
            int quantity = 1;

            // Act
            var result = _productService.CalculateTotalPrice(product, quantity);

            // Assert
            Assert.Equal(500000, result);
        }

        #endregion

        #region Test Lấy Sản Phẩm Theo ID (Get Product By Id)

        [Fact]
        public async Task GetProductById_WhenProductExists_ReturnsProduct()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Electronics" };
            _context.Categories.Add(category);
            
            var product = new Product 
            { 
                Name = "Laptop Dell", 
                Price = 15000000, 
                CategoryId = 1 
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductById(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Laptop Dell", result.Name);
            Assert.Equal(15000000, result.Price);
        }

        [Fact]
        public async Task GetProductById_WhenProductNotExists_ReturnsNull()
        {
            // Act
            var result = await _productService.GetProductById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductById_WithCategory_ReturnsProductWithCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Electronics" };
            _context.Categories.Add(category);
            
            var product = new Product 
            { 
                Name = "Laptop", 
                Price = 15000000, 
                CategoryId = 1 
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductById(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Category);
            Assert.Equal("Electronics", result.Category.Name);
        }

        #endregion

        #region Test Tính Giá Sản Phẩm Edge Cases (Calculate Price Edge Cases)

        [Fact]
        public void CalculateTotalPrice_WithNegativeQuantity_ThrowsException()
        {
            // Arrange
            var product = new Product
            {
                Name = "Laptop Dell",
                Price = 10000000
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _productService.CalculateTotalPrice(product, -1));
        }

        [Fact]
        public void CalculateTotalPrice_WithNullProduct_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                _productService.CalculateTotalPrice(null!, 3));
        }

        [Fact]
        public void CalculateTotalPrice_WithNegativePrice_ThrowsException()
        {
            // Arrange
            var product = new Product
            {
                Name = "Laptop Dell",
                Price = -10000000
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _productService.CalculateTotalPrice(product, 3));
        }

        #endregion
    }

}
