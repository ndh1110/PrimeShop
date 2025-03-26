using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using _1298_DUYHUNG.Models;
using _1298_DUYHUNG.Data;
using Microsoft.EntityFrameworkCore;
using _1298_DUYHUNG.Extensions;
using System.Text.Json;

namespace _1298_DUYHUNG.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        private readonly Dictionary<string, string> _keywordToCategoryMap = new Dictionary<string, string>
        {
            { "chuột", "Mice" },
            { "mouse", "Mice" },
            { "chuot", "Mice" },
            { "tai nghe", "Headsets" },
            { "headset", "Headsets" },
            { "headphone", "Headsets" },
            { "loa", "Speakers" },
            { "speaker", "Speakers" },
            { "laptop", "Laptops" },
            { "bàn phím", "Keyboards" },
            { "keyboard", "Keyboards" },
            { "ghế", "Chairs" },
            { "chair", "Chairs" },
            { "phụ kiện", "Accessories" },
            { "accessory", "Accessories" },
            { "đế tản nhiệt", "Docks" },
            { "dock", "Docks" },
            { "mic", "Microphones" },
            { "microphone", "Microphones" },
            { "tay cầm", "Controllers" },
            { "controller", "Controllers" }
        };

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Home()
        {
            UpdateCartCount();
            return View();
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 12; // Số sản phẩm mỗi trang
            var totalProducts = _context.Products.Count(); // Tổng số sản phẩm
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize); // Tổng số trang

            // Đảm bảo page hợp lệ
            page = Math.Max(1, page);
            page = Math.Min(page, totalPages > 0 ? totalPages : 1);

            // Lấy sản phẩm theo trang
            var products = _context.Products
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Truyền thông tin phân trang sang view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            UpdateCartCount();
            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            UpdateCartCount();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            UpdateCartCount();
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            UpdateCartCount();
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _context.Products.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Description = product.Description;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            UpdateCartCount();
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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

        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            UpdateCartCount();
            return View(product);
        }

        public IActionResult Privacy()
        {
            UpdateCartCount();
            return View();
        }

        [AllowAnonymous]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                UpdateCartCount();
                return View("Search", new List<Product>());
            }

            query = query.ToLower().Trim();
            var matchedCategory = _keywordToCategoryMap
                .Where(kv => kv.Key.ToLower() == query)
                .Select(kv => kv.Value)
                .FirstOrDefault();

            var products = new List<Product>();

            if (matchedCategory != null)
            {
                products = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Category != null && p.Category.Name == matchedCategory)
                    .ToList();
            }
            else
            {
                products = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Name.ToLower().Contains(query) || 
                                (p.Category != null && p.Category.Name.ToLower().Contains(query)))
                    .ToList();
            }

            UpdateCartCount();
            return View("Search", products);
        }

        [Authorize]
        public IActionResult AddToCart(int id, int quantity)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == id);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            UpdateCartCount();
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            UpdateCartCount();
            return View(cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                if (cartItem.Quantity <= 0)
                {
                    cart.Remove(cartItem);
                }
                HttpContext.Session.SetObjectAsJson("Cart", cart);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [Authorize]
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [Authorize]
        [HttpPost]
        public IActionResult RemoveSelectedFromCart([FromBody] RemoveSelectedRequest request)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            cart.RemoveAll(c => request.ProductIds.Contains(c.ProductId));
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Json(new { success = true });
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var count = cart.Sum(c => c.Quantity);
            return Json(new { count = count });
        }

        [Authorize]
        [HttpPost]
        public IActionResult Checkout(string selectedProductIds)
        {
            if (string.IsNullOrEmpty(selectedProductIds))
            {
                return RedirectToAction("Cart");
            }

            var productIds = JsonSerializer.Deserialize<List<int>>(selectedProductIds);
            if (productIds == null)
            {
                return RedirectToAction("Cart");
            }
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var selectedItems = cart.Where(item => productIds.Contains(item.ProductId)).ToList();

            if (!selectedItems.Any())
            {
                return RedirectToAction("Cart");
            }

            var model = new CheckoutViewModel
            {
                SelectedItems = selectedItems
            };

            UpdateCartCount();
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ConfirmCheckout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var selectedItems = cart.Where(item => model.SelectedItems.Any(si => si.ProductId == item.ProductId)).ToList();

            if (!selectedItems.Any())
            {
                return RedirectToAction("Cart");
            }

            cart.RemoveAll(item => selectedItems.Any(si => si.ProductId == item.ProductId));
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("OrderConfirmation");
        }

        [Authorize]
        public IActionResult OrderConfirmation()
        {
            UpdateCartCount();
            return View();
        }

        private void UpdateCartCount()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
                ViewBag.CartCount = cart.Sum(c => c.Quantity);
            }
            else
            {
                ViewBag.CartCount = 0;
            }
        }
    }

    public class RemoveSelectedRequest
    {
        public List<int> ProductIds { get; set; } = new List<int>();
    }
}