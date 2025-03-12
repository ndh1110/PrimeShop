using Microsoft.EntityFrameworkCore;
using _1298_DUYHUNG.Models;

namespace _1298_DUYHUNG.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình độ chính xác và tỷ lệ cho Price
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2); // 18 chữ số, 2 chữ số sau dấu chấm
            base.OnModelCreating(modelBuilder);
        }
    }
}