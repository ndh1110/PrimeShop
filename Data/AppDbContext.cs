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
    }
}