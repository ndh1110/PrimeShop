using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using _1298_DUYHUNG.Data;
using _1298_DUYHUNG.Models;

namespace _1298_DUYHUNG.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy danh sách danh mục và số lượng sản phẩm
            var categoriesWithCount = _context.Categories
                .Select(c => new
                {
                    Category = c,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.Id)
                })
                .ToList();

            // Phân loại danh mục
            var mainCategories = categoriesWithCount
                .Where(c => c.ProductCount >= 2)
                .Select(c => c.Category)
                .ToList();

            var otherCategories = categoriesWithCount
                .Where(c => c.ProductCount < 2)
                .Select(c => c.Category)
                .ToList();

            // Lưu danh sách "otherCategories" vào TempData để sử dụng trong action OtherCategories
            TempData["OtherCategories"] = System.Text.Json.JsonSerializer.Serialize(otherCategories);

            return View(mainCategories);
        }

        public IActionResult OtherCategories()
        {
            // Lấy danh sách danh mục "Sản phẩm khác" từ TempData
            var otherCategoriesJson = TempData["OtherCategories"]?.ToString();
            var otherCategories = string.IsNullOrEmpty(otherCategoriesJson)
                ? new List<Category>()
                : System.Text.Json.JsonSerializer.Deserialize<List<Category>>(otherCategoriesJson);

            // Lấy danh sách ID của các danh mục "Sản phẩm khác"
            var otherCategoryIds = otherCategories != null ? otherCategories.Select(c => c.Id).ToList() : new List<int>();

            // Lấy tất cả sản phẩm thuộc các danh mục "Sản phẩm khác"
            var products = _context.Products
                .Where(p => otherCategoryIds.Contains(p.CategoryId))
                .ToList();

            // Truyền tiêu đề "Sản phẩm Khác" vào ViewBag
            ViewBag.CategoryName = "Khác";
            ViewBag.IsCategoryView = true; // Đánh dấu đây là view theo danh mục

            // Sử dụng view SearchResults để hiển thị sản phẩm
            return View("~/Views/Home/Search.cshtml", products);
        }

        public IActionResult ProductsByCategory(int id)
        {
            // Lấy danh mục theo id
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            // Lấy danh sách sản phẩm thuộc danh mục
            var products = _context.Products
                .Where(p => p.CategoryId == id)
                .ToList();

            // Truyền tên danh mục vào ViewBag để hiển thị tiêu đề
            ViewBag.CategoryName = category.Name;
            ViewBag.IsCategoryView = true; // Đánh dấu đây là view theo danh mục

            // Sử dụng view SearchResults để hiển thị sản phẩm
            return View("~/Views/Home/Search.cshtml", products);
        }
    }
}