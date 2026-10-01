using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QA.Areas.Admin.Models;
using QA.Helpers;
using QA.Models;
using QA.Services;

namespace QA.Areas.Admin.Controllers
{
    public class ProductsController : AdminControllerBase
    {
        private const int PageSize = 10;
        private const string ImageFolder = "products";

        private readonly ClothingStoreDbContext _context;
        private readonly IImageService _imageService;

        public ProductsController(ClothingStoreDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET: /Admin/Products?q=&categoryId=&status=&page=
        public async Task<IActionResult> Index(string? q, int? categoryId, string? status, int page = 1)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.ProductName.Contains(q.Trim()));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            query = status switch
            {
                "active" => query.Where(p => p.IsActive),
                "hidden" => query.Where(p => !p.IsActive),
                "lowstock" => query.Where(p => p.StockQuantity <= ShopSettings.LowStockThreshold),
                "featured" => query.Where(p => p.IsFeatured),
                _ => query
            };

            ViewBag.Q = q;
            ViewBag.CategoryId = categoryId;
            ViewBag.Status = status;
            ViewBag.Categories = new SelectList(
                await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryId", "CategoryName", categoryId);
            ViewData["Title"] = "Quản lý sản phẩm";

            var products = await PagedList<Product>.CreateAsync(
                query.OrderByDescending(p => p.ProductId), page, PageSize);

            return View(products);
        }

        // ============================
        // CREATE
        // ============================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadLookupsAsync();
            ViewData["Title"] = "Thêm sản phẩm";

            return View("Form", new ProductFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            await HandleImageAsync(model);

            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync();
                ViewData["Title"] = "Thêm sản phẩm";
                return View("Form", model);
            }

            var product = new Product { CreatedAt = DateTime.Now };
            MapToEntity(model, product);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            Success($"Đã thêm sản phẩm \"{product.ProductName}\".");

            return RedirectToAction(nameof(Index));
        }

        // ============================
        // EDIT
        // ============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.Products.FindAsync(id);

            if (p == null)
            {
                return NotFound();
            }

            await LoadLookupsAsync();
            ViewData["Title"] = "Sửa sản phẩm";

            return View("Form", new ProductFormViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                Description = p.Description,
                Price = p.Price,
                SalePrice = p.SalePrice,
                StockQuantity = p.StockQuantity,
                Size = p.Size,
                Color = p.Color,
                Material = p.Material,
                ImageUrl = p.ImageUrl,
                IsFeatured = p.IsFeatured,
                IsActive = p.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            string? oldImage = product.ImageUrl;

            await HandleImageAsync(model);

            if (!ModelState.IsValid)
            {
                model.ProductId = id;
                await LoadLookupsAsync();
                ViewData["Title"] = "Sửa sản phẩm";
                return View("Form", model);
            }

            MapToEntity(model, product);
            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            if (oldImage != product.ImageUrl)
            {
                _imageService.Delete(oldImage);
            }

            Success($"Đã cập nhật sản phẩm \"{product.ProductName}\".");

            return RedirectToAction(nameof(Index));
        }

        // ============================
        // DELETE
        // ============================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.HasOrders = await _context.OrderDetails.AnyAsync(d => d.ProductId == id);
            ViewData["Title"] = "Xóa sản phẩm";

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Sản phẩm đã có trong đơn hàng => chỉ ẩn để giữ lịch sử đơn
            if (await _context.OrderDetails.AnyAsync(d => d.ProductId == id))
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                Success($"Sản phẩm \"{product.ProductName}\" đã có đơn hàng nên được chuyển sang trạng thái ẩn.");
                return RedirectToAction(nameof(Index));
            }

            await _context.CartItems.Where(i => i.ProductId == id).ExecuteDeleteAsync();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _imageService.Delete(product.ImageUrl);

            Success($"Đã xóa sản phẩm \"{product.ProductName}\".");

            return RedirectToAction(nameof(Index));
        }

        // ============================
        // QUICK TOGGLES
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, string? returnUrl)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                product.IsActive = !product.IsActive;
                product.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                Success($"\"{product.ProductName}\" đã {(product.IsActive ? "hiển thị" : "ẩn")}.");
            }

            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int id, string? returnUrl)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                product.IsFeatured = !product.IsFeatured;
                product.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectBack(returnUrl);
        }

        // ============================
        // HELPERS
        // ============================
        private async Task LoadLookupsAsync()
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryId", "CategoryName");

            ViewBag.Brands = new SelectList(
                await _context.Brands.OrderBy(b => b.BrandName).ToListAsync(),
                "BrandId", "BrandName");
        }

        // Nếu có file upload thì lưu và gán vào ImageUrl
        private async Task HandleImageAsync(ProductFormViewModel model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                return;
            }

            var result = await _imageService.SaveAsync(model.ImageFile, ImageFolder);

            if (result.Success)
            {
                model.ImageUrl = result.Data;
            }
            else
            {
                ModelState.AddModelError(nameof(model.ImageFile), result.Message);
            }
        }

        private static void MapToEntity(ProductFormViewModel model, Product product)
        {
            product.ProductName = model.ProductName.Trim();
            product.CategoryId = model.CategoryId!.Value;
            product.BrandId = model.BrandId;
            product.Description = model.Description;
            product.Price = model.Price!.Value;
            product.SalePrice = model.SalePrice > 0 ? model.SalePrice : null;
            product.StockQuantity = model.StockQuantity!.Value;
            product.Size = model.Size;
            product.Color = model.Color;
            product.Material = model.Material;
            product.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();
            product.IsFeatured = model.IsFeatured;
            product.IsActive = model.IsActive;
        }

        private IActionResult RedirectBack(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
