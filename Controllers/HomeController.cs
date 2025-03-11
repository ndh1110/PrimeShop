using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using _1298_DUYHUNG.Models;
using _1298_DUYHUNG.Data;

namespace _1298_DUYHUNG.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        // Inject AppDbContext qua constructor
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Home()
        {
            return View();
        }

        public IActionResult Index()
        {
            // Lấy 5 sản phẩm đầu tiên từ database
            var initialProducts = _context.Products.Take(5).ToList();
            return View(initialProducts);
        }

        [HttpGet]
        public IActionResult GetProducts(int page = 1, int pageSize = 5)
        {
            // Bỏ qua các sản phẩm đã hiển thị và lấy pageSize sản phẩm tiếp theo
            var products = _context.Products
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
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // Action để hiển thị form chỉnh sửa sản phẩm
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
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
                var existingProduct = _context.Products.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                // Cập nhật thông tin sản phẩm
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Description = product.Description;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // Action để xóa sản phẩm
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}