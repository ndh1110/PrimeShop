using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using _1298_DUYHUNG.Data;
using _1298_DUYHUNG.Models;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext
builder.Services.AddControllers(); // Hỗ trợ API controllers
builder.Services.AddControllersWithViews(); // Hỗ trợ MVC (cho các trang web hiện tại)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// (Tùy chọn) Thêm Swagger để kiểm tra API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cấu hình Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Đăng ký UserManager và RoleManager
builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed dữ liệu
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    // Tạo vai trò
    string[] roleNames = { "Admin", "Employee", "Customer", "Company" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
    
    // Tạo tài khoản Admin
    var adminUser = await userManager.FindByEmailAsync("admin@example.com");
    if (adminUser == null)
    {
        adminUser = new User 
        { 
            UserName = "admin", 
            Email = "admin@example.com",
            FullName = "Admin User",
            Address = "123 Admin Street",
            PhoneNumber = "1234567890",
            DateCreated = DateTime.UtcNow
        };
        await userManager.CreateAsync(adminUser, "AdminPassword123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    var employeeUser = await userManager.FindByEmailAsync("employee@example.com");
    if (employeeUser == null)
    {
        employeeUser = new User 
        { 
            UserName = "employee", 
            Email = "employee@example.com",
            FullName = "Employee User",
            Address = "456 Employee Lane",
            PhoneNumber = "9876543210",
            DateCreated = DateTime.UtcNow
        };
        await userManager.CreateAsync(employeeUser, "EmployeePassword123!");
        await userManager.AddToRoleAsync(employeeUser, "Employee");
    }

    var customerUser = await userManager.FindByEmailAsync("customer@example.com");
    if (customerUser == null)
    {
        customerUser = new User 
        { 
            UserName = "customer", 
            Email = "customer@example.com",
            FullName = "Customer User",
            Address = "789 Customer Blvd",
            PhoneNumber = "1122334455",
            DateCreated = DateTime.UtcNow
        };
        await userManager.CreateAsync(customerUser, "CustomerPassword123!");
        await userManager.AddToRoleAsync(customerUser, "Customer");
    }

    var companyUser = await userManager.FindByEmailAsync("company@example.com");
    if (companyUser == null)
    {
        companyUser = new User 
        { 
            UserName = "company", 
            Email = "company@example.com",
            FullName = "Company User",
            Address = "101 Company Ave",
            PhoneNumber = "5566778899",
            DateCreated = DateTime.UtcNow
        };
        await userManager.CreateAsync(companyUser, "CompanyPassword123!");
        await userManager.AddToRoleAsync(companyUser, "Company");
    }

    // Seed dữ liệu cho Categories
    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category { Name = "Laptops" },    // Id = 1
            new Category { Name = "Mice" },       // Id = 2
            new Category { Name = "Keyboards" },  // Id = 3
            new Category { Name = "Headsets" },   // Id = 4
            new Category { Name = "Controllers" },// Id = 5
            new Category { Name = "Accessories" },// Id = 6
            new Category { Name = "Chairs" },     // Id = 7
            new Category { Name = "Speakers" },   // Id = 8
            new Category { Name = "Docks" },      // Id = 9
            new Category { Name = "Microphones" } // Id = 10
        );
        context.SaveChanges();
    }

    // Seed dữ liệu cho Products
    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new Product { Name = "Razer Blade 16 (2024)", Price = 2800, ImageUrl = "https://m.media-amazon.com/images/I/81u7NtA6QML._AC_SL1500_.jpg", Description = "Laptop chơi game cao cấp với RTX 4090.", CategoryId = 1 },
            new Product { Name = "Razer Blade 14 (2024)", Price = 2000, ImageUrl = "https://d28jzcg6y4v9j1.cloudfront.net/media/social/articles/2024/1/9/blade-14-2024-studio-4-thinkpro.png", Description = "Laptop chơi game nhỏ gọn với Ryzen 9.", CategoryId = 1 },
            new Product { Name = "Razer Viper V3 Pro", Price = 1590.99m, ImageUrl = "https://laptopworld.vn/media/product/17560_48699_razer_viper_v3_pro_white.jpg", Description = "Chuột eSport siêu nhẹ, cảm biến 35K DPI.", CategoryId = 2 },
            new Product { Name = "Razer DeathAdder V3 Pro", Price = 1490.99m, ImageUrl = "https://cdn.ankhang.vn/media/product/23271_chuot_vi_tinh_razer_deathadder_v3_pro.jpg", Description = "Chuột chơi game không dây, cảm biến 30K DPI.", CategoryId = 2 },
            new Product { Name = "Razer BlackWidow V4 Pro", Price = 2290.99m, ImageUrl = "https://cdn.ankhang.vn/media/product/23534_ban_phim_razer_blackwidow_v4_pro_yellow_switch_1.jpg", Description = "Bàn phím cơ với Chroma RGB, phím đa năng.", CategoryId = 3 },
            new Product { Name = "Razer Huntsman V3 Pro", Price = 1249.99m, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn9GcTqnMRg8mVKJUsVfIL6AIZm5N2UaCW1lYe6eg&s", Description = "Bàn phím quang học với phím analog.", CategoryId = 3 },
            new Product { Name = "Razer BlackShark V2 Pro (2023)", Price = 1990.99m, ImageUrl = "https://file.hstatic.net/200000637319/file/91dar2zue_l._ac_sl1500__15d2cfa1db154fc4bc53d15ad3528a88_grande.jpg", Description = "Tai nghe không dây cho eSport, âm thanh THX.", CategoryId = 4 },
            new Product { Name = "Razer Kraken V3 HyperSense", Price = 1290.99m, ImageUrl = "https://file.hstatic.net/200000637319/file/pz1_10d031662e9a40b1a86892635d2e9112_grande.jpg", Description = "Tai nghe có rung phản hồi, Chroma RGB.", CategoryId = 4 },
            new Product { Name = "Razer Barracuda X", Price = 1990.99m, ImageUrl = "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/a/tai-nghe-chup-tai-razer-barracuda-x-.png", Description = "Tai nghe đa nền tảng không dây.", CategoryId = 4 },
            new Product { Name = "Razer Kishi Ultra", Price = 1490.99m, ImageUrl = "https://assets2.razerzone.com/images/pnx.assets/88482da1d03f7a8c4b6392da6500d5fe/razer-kishi-ultra-chroma-customization-desktop.webp", Description = "Tay cầm chơi game di động cho điện thoại.", CategoryId = 5 },
            new Product { Name = "Razer Wolverine V3 Pro", Price = 1990.99m, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn9GcTY9qTYF8xOsyMjIca46PR9e8d0zhUrHTJSPg&s", Description = "Tay cầm chơi game chuyên nghiệp cho Xbox/PC.", CategoryId = 5 },
            new Product { Name = "Razer Firefly V2 Pro", Price = 1990.99m, ImageUrl = "https://techspace.vn/wp-content/uploads/2024/04/1-51-1024x538.webp", Description = "Bàn di chuột RGB với 15 vùng chiếu sáng.", CategoryId = 6 },
            new Product { Name = "Razer Iskur V2", Price = 6490.99m, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn9GcRY5-eMuY0MV4GRozz0tcItlGe2Xsop1YOUcg&s", Description = "Ghế chơi game công thái học, hỗ trợ thắt lưng.", CategoryId = 7 },
            new Product { Name = "Razer Nommo V2 Pro", Price = 4490.99m, ImageUrl = "https://file.hstatic.net/200000722513/file/gearvn-loa-razer-nommo-v2-pro-1_b7d81ebec2f045649baad0b940aadc0e_1024x1024.jpg", Description = "Loa chơi game với subwoofer, Chroma RGB.", CategoryId = 8 },
            new Product { Name = "Razer USB 4 Dock", Price = 2990.99m, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn9GcRASItvr57_3Oj-XSXHJGd5cQ-Uhlxm7zK3_Q&s", Description = "Dock mở rộng cho laptop, hỗ trợ màn hình kép.", CategoryId = 9 },
            new Product { Name = "Razer Moray", Price = 1290.99m, ImageUrl = "https://cdn.tgdd.vn/Files/2023/07/03/1536678/tren-tay-razer-moray-thumb-030723-110100-600x400.jpg", Description = "Tai nghe in-ear cho streaming và chơi game.", CategoryId = 4 },
            new Product { Name = "Razer Basilisk V3 Pro", Price = 1590.99m, ImageUrl = "https://techspace.vn/wp-content/uploads/2022/09/Chuot-Razer-Basilisk-V3-Pro-9.jpg", Description = "Chuột không dây đa năng, 13 nút lập trình.", CategoryId = 2 },
            new Product { Name = "Razer Ornata V3", Price = 1690.99m, ImageUrl = "https://product.hstatic.net/1000287389/product/ix-images-container_h91_ha4_9413534318622_220623-ornata-v3-1500x1000-1_de50e44e215840e9a53899de2e2e33f0.jpg", Description = "Bàn phím lai cơ-màng, Chroma RGB.", CategoryId = 3 },
            new Product { Name = "Razer Seiren V3 Chroma", Price = 1290.99m, ImageUrl = "https://file.hstatic.net/200000722513/file/rezer_seiren_v3_chroma.jpg", Description = "Micro USB với đèn RGB, chất lượng phòng thu.", CategoryId = 10 },
            new Product { Name = "Razer Leviathan V2", Price = 1249.99m, ImageUrl = "https://cdn2.cellphones.com.vn/x/media/catalog/product/l/o/loa-soundbar-razer-leviathan-v2-x.png", Description = "Soundbar chơi game với subwoofer, âm thanh vòm.", CategoryId = 8 }
        );
        context.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection(); // Chỉ bật HttpsRedirection trong môi trường không phải Development
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "admin_area",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Định tuyến cho API
app.MapControllers(); // Hỗ trợ API routes (như /api/products)
    
app.Run();