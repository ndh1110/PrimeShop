using Microsoft.EntityFrameworkCore;
using _1298_DUYHUNG.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace _1298_DUYHUNG.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Cấu hình cho thuộc tính Price của Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2); // 18 chữ số tổng cộng, 2 chữ số thập phân

            // Cấu hình khác (nếu có)
        }
    }
}