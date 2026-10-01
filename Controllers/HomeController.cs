using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QA.Helpers;
using QA.Models;
using QA.ViewModels;

namespace QA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ClothingStoreDbContext _context;

        public HomeController(ClothingStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OnlyVisible();

            var model = new HomeViewModel
            {
                Categories = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CategoryId)
                    .ToListAsync(),

                FeaturedProducts = await products
                    .Where(p => p.IsFeatured)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .ToListAsync(),

                NewProducts = await products
                    .OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.ProductId)
                    .Take(4)
                    .ToListAsync(),

                SaleProducts = await products
                    .WhereOnSale()
                    .OrderBy(p => p.SalePrice / p.Price)
                    .Take(4)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Home/StatusCode")]
        public IActionResult StatusCodePage(int code)
        {
            ViewData["Code"] = code;
            return View("StatusCode");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
