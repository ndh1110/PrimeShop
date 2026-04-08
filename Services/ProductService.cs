using Microsoft.EntityFrameworkCore;
using _1298_DUYHUNG.Data;
using _1298_DUYHUNG.Models;

namespace _1298_DUYHUNG.Services
{
    public class ProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetProductById(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> GetAllProducts()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }

        public async Task<Product> AddProduct(Product product)
        {
            if (product.CategoryId != 0)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
                if (!categoryExists) throw new InvalidOperationException("Danh mục không tồn tại.");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public decimal CalculateTotalPrice(Product product, int quantity)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (quantity < 0) throw new ArgumentException("Số lượng không được âm");
            if (product.Price < 0) throw new ArgumentException("Giá sản phẩm không hợp lệ");
            
            return product.Price * quantity;
        }
    }
}