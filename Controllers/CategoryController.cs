using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using _1298_DUYHUNG.Models;

namespace _1298_DUYHUNG.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Laptops" },
                new Category { Id = 2, Name = "Mice" },
                new Category { Id = 3, Name = "Keyboards" },
                new Category { Id = 4, Name = "Headsets" }
            };
            return View(categories);
        }
    }
}