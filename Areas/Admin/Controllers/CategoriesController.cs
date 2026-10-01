using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QA.Areas.Admin.Models;
using QA.Models;
using QA.Services;

namespace QA.Areas.Admin.Controllers
{
    public class CategoriesController : AdminControllerBase
    {
        private const string ImageFolder = "categories";

        private readonly ClothingStoreDbContext _context;
        private readonly IImageService _imageService;

        public CategoriesController(ClothingStoreDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Danh mục";

            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm danh mục";
            return View("Form", new CategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            await ValidateAsync(model);

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Thêm danh mục";
                return View("Form", model);
            }

            var category = new Category { CreatedAt = DateTime.Now };
            Map(model, category);

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            Success($"Đã thêm danh mục \"{category.CategoryName}\".");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _context.Categories.FindAsync(id);

            if (c == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Sửa danh mục";

            return View("Form", new CategoryFormViewModel
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            model.CategoryId = id;
            string? oldImage = category.ImageUrl;

            await ValidateAsync(model);

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Sửa danh mục";
                return View("Form", model);
            }

            Map(model, category);
            await _context.SaveChangesAsync();

            if (oldImage != category.ImageUrl)
            {
                _imageService.Delete(oldImage);
            }

            Success($"Đã cập nhật danh mục \"{category.CategoryName}\".");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (await _context.Products.AnyAsync(p => p.CategoryId == id))
            {
                Error($"Danh mục \"{category.CategoryName}\" đang có sản phẩm. Hãy chuyển sản phẩm sang danh mục khác hoặc ẩn danh mục.");
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _imageService.Delete(category.ImageUrl);

            Success($"Đã xóa danh mục \"{category.CategoryName}\".");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category != null)
            {
                category.IsActive = !category.IsActive;
                await _context.SaveChangesAsync();

                Success($"Danh mục \"{category.CategoryName}\" đã {(category.IsActive ? "hiển thị" : "ẩn")}.");
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateAsync(CategoryFormViewModel model)
        {
            if (await _context.Categories.AnyAsync(c =>
                    c.CategoryName == model.CategoryName.Trim() && c.CategoryId != model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryName), "Tên danh mục đã tồn tại.");
            }

            if (model.ImageFile is { Length: > 0 } && ModelState.IsValid)
            {
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
        }

        private static void Map(CategoryFormViewModel model, Category category)
        {
            category.CategoryName = model.CategoryName.Trim();
            category.Description = model.Description;
            category.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();
            category.IsActive = model.IsActive;
        }
    }
}
