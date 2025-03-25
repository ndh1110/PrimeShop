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

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Home()
        {
            UpdateCartCount();
            return View();
        }

        public IActionResult Index()
        {
            var initialProducts = _context.Products.Take(5).ToList();
            UpdateCartCount();
            return View(initialProducts);
        }

        [HttpGet]
        public IActionResult GetProducts(int page = 1, int pageSize = 5)
        {
            var products = _context.Products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return Json(products);
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
            if (string.IsNullOrEmpty(query))
            {
                UpdateCartCount();
                return View(new List<Product>());
            }

            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(query) || p.Category.Name.Contains(query))
                .ToList();

            UpdateCartCount();
            return View(products);
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            UpdateCartCount();
            return View(cart);
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
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

        [AllowAnonymous]
        [HttpPost]
        public IActionResult RemoveSelectedFromCart([FromBody] RemoveSelectedRequest request)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            cart.RemoveAll(c => request.ProductIds.Contains(c.ProductId));
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Json(new { success = true });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var count = cart.Sum(c => c.Quantity);
            return Json(new { count = count });
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Checkout(string selectedProductIds)
        {
            if (string.IsNullOrEmpty(selectedProductIds))
            {
                return RedirectToAction("Cart");
            }

            var productIds = JsonSerializer.Deserialize<List<int>>(selectedProductIds);
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

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ConfirmCheckout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            // Lấy danh sách sản phẩm đã chọn từ giỏ hàng
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var selectedItems = cart.Where(item => model.SelectedItems.Any(si => si.ProductId == item.ProductId)).ToList();

            if (!selectedItems.Any())
            {
                return RedirectToAction("Cart");
            }

            // Xử lý đơn hàng (lưu vào database, gửi email, v.v.)
            // Ở đây tôi sẽ chỉ xóa các sản phẩm đã chọn khỏi giỏ hàng
            cart.RemoveAll(item => selectedItems.Any(si => si.ProductId == item.ProductId));
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // Chuyển hướng đến trang xác nhận đơn hàng
            return RedirectToAction("OrderConfirmation");
        }

        [AllowAnonymous]
        public IActionResult OrderConfirmation()
        {
            UpdateCartCount();
            return View();
        }

        private void UpdateCartCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            ViewBag.CartCount = cart.Sum(c => c.Quantity);
        }
    }

    public class RemoveSelectedRequest
    {
        public List<int> ProductIds { get; set; }
    }
}