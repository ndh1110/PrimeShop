using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using _1298_DUYHUNG.Models;

namespace _1298_DUYHUNG.Controllers
{
    public class HomeController : Controller
    {
        // Danh sách sản phẩm (lưu trữ tĩnh, thay bằng database nếu cần)
        private static readonly List<Product> AllProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Razer Blade 16 (2024)", Price = 2800, ImageUrl = "https://m.media-amazon.com/images/I/81u7NtA6QML._AC_SL1500_.jpg", Description = "Laptop chơi game cao cấp với RTX 4090." },
            new Product { Id = 2, Name = "Razer Blade 14 (2024)", Price = 2000, ImageUrl = "https://d28jzcg6y4v9j1.cloudfront.net/media/social/articles/2024/1/9/blade-14-2024-studio-4-thinkpro.png", Description = "Laptop chơi game nhỏ gọn với Ryzen 9." },
            new Product { Id = 3, Name = "Razer Viper V3 Pro", Price = 159.99m, ImageUrl = "https://laptopworld.vn/media/product/17560_48699_razer_viper_v3_pro_white.jpg", Description = "Chuột eSport siêu nhẹ, cảm biến 35K DPI." },
            new Product { Id = 4, Name = "Razer DeathAdder V3 Pro", Price = 149.99m, ImageUrl = "https://cdn.ankhang.vn/media/product/23271_chuot_vi_tinh_razer_deathadder_v3_pro.jpg", Description = "Chuột chơi game không dây, cảm biến 30K DPI." },
            new Product { Id = 5, Name = "Razer BlackWidow V4 Pro", Price = 229.99m, ImageUrl = "https://cdn.ankhang.vn/media/product/23534_ban_phim_razer_blackwidow_v4_pro_yellow_switch_1.jpg", Description = "Bàn phím cơ với Chroma RGB, phím đa năng." },
            new Product { Id = 6, Name = "Razer Huntsman V3 Pro", Price = 249.99m, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTqnMRg8mVKJUsVfIL6AIZm5N2UaCW1lYe6eg&s", Description = "Bàn phím quang học với phím analog." },
            new Product { Id = 7, Name = "Razer BlackShark V2 Pro (2023)", Price = 199.99m, ImageUrl = "https://file.hstatic.net/200000637319/file/91dar2zue_l._ac_sl1500__15d2cfa1db154fc4bc53d15ad3528a88_grande.jpg", Description = "Tai nghe không dây cho eSport, âm thanh THX." },
            new Product { Id = 8, Name = "Razer Kraken V3 HyperSense", Price = 129.99m, ImageUrl = "https://file.hstatic.net/200000637319/file/pz1_10d031662e9a40b1a86892635d2e9112_grande.jpg", Description = "Tai nghe có rung phản hồi, Chroma RGB." },
            new Product { Id = 9, Name = "Razer Barracuda X", Price = 99.99m, ImageUrl = "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/a/tai-nghe-chup-tai-razer-barracuda-x-.png", Description = "Tai nghe đa nền tảng không dây." },
            new Product { Id = 10, Name = "Razer Kishi Ultra", Price = 149.99m, ImageUrl = "https://assets2.razerzone.com/images/pnx.assets/88482da1d03f7a8c4b6392da6500d5fe/razer-kishi-ultra-chroma-customization-desktop.webp", Description = "Tay cầm chơi game di động cho điện thoại." },
            new Product { Id = 11, Name = "Razer Wolverine V3 Pro", Price = 199.99m, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTY9qTYF8xOsyMjIca46PR9e8d0zhUrHTJSPg&s", Description = "Tay cầm chơi game chuyên nghiệp cho Xbox/PC." },
            new Product { Id = 12, Name = "Razer Firefly V2 Pro", Price = 99.99m, ImageUrl = "https://techspace.vn/wp-content/uploads/2024/04/1-51-1024x538.webp", Description = "Bàn di chuột RGB với 15 vùng chiếu sáng." },
            new Product { Id = 13, Name = "Razer Iskur V2", Price = 649.99m, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn9GcRY5-eMuY0MV4GRozz0tcItlGe2Xsop1YOUcg&s", Description = "Ghế chơi game công thái học, hỗ trợ thắt lưng." },
            new Product { Id = 14, Name = "Razer Nommo V2 Pro", Price = 449.99m, ImageUrl = "https://file.hstatic.net/200000722513/file/gearvn-loa-razer-nommo-v2-pro-1_b7d81ebec2f045649baad0b940aadc0e_1024x1024.jpg", Description = "Loa chơi game với subwoofer, Chroma RGB." },
            new Product { Id = 15, Name = "Razer USB 4 Dock", Price = 299.99m, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn9GcRASItvr57_3Oj-XSXHJGd5cQ-Uhlxm7zK3_Q&s", Description = "Dock mở rộng cho laptop, hỗ trợ màn hình kép." },
            new Product { Id = 16, Name = "Razer Moray", Price = 129.99m, ImageUrl = "https://cdn.tgdd.vn/Files/2023/07/03/1536678/tren-tay-razer-moray-thumb-030723-110100-600x400.jpg", Description = "Tai nghe in-ear cho streaming và chơi game." },
            new Product { Id = 17, Name = "Razer Basilisk V3 Pro", Price = 159.99m, ImageUrl = "https://techspace.vn/wp-content/uploads/2022/09/Chuot-Razer-Basilisk-V3-Pro-9.jpg", Description = "Chuột không dây đa năng, 13 nút lập trình." },
            new Product { Id = 18, Name = "Razer Ornata V3", Price = 69.99m, ImageUrl = "https://product.hstatic.net/1000287389/product/ix-images-container_h91_ha4_9413534318622_220623-ornata-v3-1500x1000-1_de50e44e215840e9a53899de2e2e33f0.jpg", Description = "Bàn phím lai cơ-màng, Chroma RGB." },
            new Product { Id = 19, Name = "Razer Seiren V3 Chroma", Price = 129.99m, ImageUrl = "https://file.hstatic.net/200000722513/file/rezer_seiren_v3_chroma.jpg", Description = "Micro USB với đèn RGB, chất lượng phòng thu." },
            new Product { Id = 20, Name = "Razer Leviathan V2", Price = 249.99m, ImageUrl = "https://cdn2.cellphones.com.vn/x/media/catalog/product/l/o/loa-soundbar-razer-leviathan-v2-x.png", Description = "Soundbar chơi game với subwoofer, âm thanh vòm." }
        };

        public IActionResult Home()
        {
            return View();
        }

        public IActionResult Index()
        {
            // Lấy 5 sản phẩm đầu tiên để hiển thị ban đầu
            var initialProducts = AllProducts.Take(5).ToList();
            return View(initialProducts);
        }

        [HttpGet]
        public IActionResult GetProducts(int page = 1, int pageSize = 5)
        {
            // Bỏ qua các sản phẩm đã hiển thị và lấy pageSize sản phẩm tiếp theo
            var products = AllProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Json(products);
        }

        // Action để hiển thị form thêm sản phẩm
        public IActionResult Create()
        {
            return View();
        }

        // Action để xử lý thêm sản phẩm
        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                // Tạo ID mới (tăng dần dựa trên ID lớn nhất hiện có)
                product.Id = AllProducts.Any() ? AllProducts.Max(p => p.Id) + 1 : 1;
                AllProducts.Add(product);
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // Action để hiển thị form chỉnh sửa sản phẩm
        public IActionResult Edit(int id)
        {
            var product = AllProducts.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Action để xử lý chỉnh sửa sản phẩm
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = AllProducts.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                // Cập nhật thông tin sản phẩm
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Description = product.Description;
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // Action để xóa sản phẩm
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = AllProducts.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            AllProducts.Remove(product);
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}