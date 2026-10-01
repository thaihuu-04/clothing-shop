using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QA.Areas.Admin.Models;
using QA.Models;
using QA.Services;

namespace QA.Areas.Admin.Controllers
{
    public class BrandsController : AdminControllerBase
    {
        private const string ImageFolder = "brands";

        private readonly ClothingStoreDbContext _context;
        private readonly IImageService _imageService;

        public BrandsController(ClothingStoreDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Thương hiệu";

            var brands = await _context.Brands
                .AsNoTracking()
                .Include(b => b.Products)
                .OrderBy(b => b.BrandName)
                .ToListAsync();

            return View(brands);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm thương hiệu";
            return View("Form", new BrandFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandFormViewModel model)
        {
            await ValidateAsync(model);

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Thêm thương hiệu";
                return View("Form", model);
            }

            var brand = new Brand();
            Map(model, brand);

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            Success($"Đã thêm thương hiệu \"{brand.BrandName}\".");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var b = await _context.Brands.FindAsync(id);

            if (b == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Sửa thương hiệu";

            return View("Form", new BrandFormViewModel
            {
                BrandId = b.BrandId,
                BrandName = b.BrandName,
                Description = b.Description,
                LogoUrl = b.LogoUrl,
                IsActive = b.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BrandFormViewModel model)
        {
            var brand = await _context.Brands.FindAsync(id);

            if (brand == null)
            {
                return NotFound();
            }

            model.BrandId = id;
            string? oldLogo = brand.LogoUrl;

            await ValidateAsync(model);

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Sửa thương hiệu";
                return View("Form", model);
            }

            Map(model, brand);
            await _context.SaveChangesAsync();

            if (oldLogo != brand.LogoUrl)
            {
                _imageService.Delete(oldLogo);
            }

            Success($"Đã cập nhật thương hiệu \"{brand.BrandName}\".");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands.FindAsync(id);

            if (brand == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (await _context.Products.AnyAsync(p => p.BrandId == id))
            {
                Error($"Thương hiệu \"{brand.BrandName}\" đang có sản phẩm, không thể xóa. Bạn có thể tắt hoạt động.");
                return RedirectToAction(nameof(Index));
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            _imageService.Delete(brand.LogoUrl);

            Success($"Đã xóa thương hiệu \"{brand.BrandName}\".");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var brand = await _context.Brands.FindAsync(id);

            if (brand != null)
            {
                brand.IsActive = !brand.IsActive;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateAsync(BrandFormViewModel model)
        {
            if (await _context.Brands.AnyAsync(b =>
                    b.BrandName == model.BrandName.Trim() && b.BrandId != model.BrandId))
            {
                ModelState.AddModelError(nameof(model.BrandName), "Tên thương hiệu đã tồn tại.");
            }

            if (model.LogoFile is { Length: > 0 } && ModelState.IsValid)
            {
                var result = await _imageService.SaveAsync(model.LogoFile, ImageFolder);

                if (result.Success)
                {
                    model.LogoUrl = result.Data;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.LogoFile), result.Message);
                }
            }
        }

        private static void Map(BrandFormViewModel model, Brand brand)
        {
            brand.BrandName = model.BrandName.Trim();
            brand.Description = model.Description;
            brand.LogoUrl = string.IsNullOrWhiteSpace(model.LogoUrl) ? null : model.LogoUrl.Trim();
            brand.IsActive = model.IsActive;
        }
    }
}
