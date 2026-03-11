using Microsoft.EntityFrameworkCore;
using _1298_DUYHUNG.Data;
using _1298_DUYHUNG.Models;

namespace _1298_DUYHUNG.Services
{
    public interface IProductService
    {
        Task<Product> AddProduct(Product product);
        Task<List<Product>> GetAllProducts();
        Task<Product?> GetProductById(int id);
        decimal CalculateTotalPrice(Product product, int quantity);
    }

    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Thêm sản phẩm mới vào database
        /// </summary>
        /// <param name="product">Thông tin sản phẩm cần thêm</param>
        /// <returns>Product đã được thêm</returns>
        public async Task<Product> AddProduct(Product product)
        {
            // Kiểm tra CategoryId có tồn tại không
            if (product.CategoryId != 0)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
                if (!categoryExists)
                {
                    throw new InvalidOperationException("Danh mục không tồn tại.");
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        /// <summary>
        /// Lấy danh sách tất cả sản phẩm
        /// </summary>
        /// <returns>Danh sách sản phẩm</returns>
        public async Task<List<Product>> GetAllProducts()
        {
            return await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy thông tin sản phẩm theo ID
        /// </summary>
        /// <param name="id">ID của sản phẩm</param>
        /// <returns>Product nếu tìm thấy, null nếu không tìm thấy</returns>
        public async Task<Product?> GetProductById(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Tính tổng giá sản phẩm theo số lượng
        /// </summary>
        /// <param name="product">Sản phẩm cần tính giá</param>
        /// <param name="quantity">Số lượng</param>
        /// <returns>Tổng giá (Price * Quantity)</returns>
        public decimal CalculateTotalPrice(Product product, int quantity)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Số lượng không được âm.", nameof(quantity));
            }

            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            if (product.Price < 0)
            {
                throw new ArgumentException("Giá sản phẩm không được âm.", nameof(product));
            }

            return product.Price * quantity;
        }
    }
}

