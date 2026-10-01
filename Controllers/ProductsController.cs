using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QA.Helpers;
using QA.Models;
using QA.ViewModels;

namespace QA.Controllers
{
    public class ProductsController : Controller
    {
        private const int PageSize = 12;

        private readonly ClothingStoreDbContext _context;

        public ProductsController(ClothingStoreDbContext context)
        {
            _context = context;
        }

        // GET: /Products?q=&categoryId=&brandId=&minPrice=&maxPrice=&onSale=&sort=&page=
        public async Task<IActionResult> Index([FromQuery] ProductFilter filter)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OnlyVisible();

            if (!string.IsNullOrWhiteSpace(filter.Q))
            {
                string keyword = filter.Q.Trim();
                query = query.Where(p =>
                    p.ProductName.Contains(keyword) ||
                    (p.Description != null && p.Description.Contains(keyword)) ||
                    (p.Brand != null && p.Brand.BrandName.Contains(keyword)));
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId);
            }

            if (filter.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == filter.BrandId);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.WherePriceFrom(filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.WherePriceTo(filter.MaxPrice.Value);
            }

            if (filter.OnSale)
            {
                query = query.WhereOnSale();
            }

            var model = new ProductListViewModel
            {
                Filter = filter,
                Products = await PagedList<Product>.CreateAsync(query.ApplySort(filter.Sort), filter.Page, PageSize),
                Categories = await _context.Categories.AsNoTracking()
                    .Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync(),
                Brands = await _context.Brands.AsNoTracking()
                    .Where(b => b.IsActive).OrderBy(b => b.BrandName).ToListAsync()
            };

            return View(model);
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OnlyVisible()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var related = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OnlyVisible()
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            return View(new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = related
            });
        }
    }
}
