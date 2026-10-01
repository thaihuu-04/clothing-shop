using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QA.Helpers;
using QA.Models;
using QA.ViewModels;
using System.Security.Claims;

namespace QA.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ClothingStoreDbContext _context;

        public ProfileController(ClothingStoreDbContext context)
        {
            _context = context;
        }

        // ============================
        // HỒ SƠ
        // ============================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _context.Users.FindAsync(User.GetUserId());

            if (user == null)
            {
                return NotFound();
            }

            return View(new ProfileViewModel
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _context.Users.FindAsync(User.GetUserId());

            if (user == null)
            {
                return NotFound();
            }

            model.Username = user.Username;
            model.CreatedAt = user.CreatedAt;

            if (model.Gender != null && !ProfileViewModel.Genders.Contains(model.Gender))
            {
                ModelState.AddModelError(nameof(model.Gender), "Giới tính không hợp lệ.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != user.UserId))
            {
                ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim();
            user.Phone = model.Phone;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;

            await _context.SaveChangesAsync();
            await RefreshSignInAsync(user);

            TempData["Success"] = "Đã cập nhật thông tin cá nhân.";

            return RedirectToAction(nameof(Index));
        }

        // ============================
        // ĐỔI MẬT KHẨU
        // ============================
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(User.GetUserId());

            if (user == null)
            {
                return NotFound();
            }

            if (user.PasswordHash != Md5Helper.Hash(model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            user.PasswordHash = Md5Helper.Hash(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công.";

            return RedirectToAction(nameof(Index));
        }

        // ============================
        // SỔ ĐỊA CHỈ
        // ============================
        [HttpGet]
        public async Task<IActionResult> Addresses()
        {
            int userId = User.GetUserId();

            var addresses = await _context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.AddressId)
                .ToListAsync();

            return View(addresses);
        }

        [HttpGet]
        public IActionResult CreateAddress(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View("AddressForm", new AddressViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAddress(AddressViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View("AddressForm", model);
            }

            int userId = User.GetUserId();
            bool isFirst = !await _context.Addresses.AnyAsync(a => a.UserId == userId);

            var address = new Address { UserId = userId };
            MapAddress(model, address);

            // Địa chỉ đầu tiên luôn là mặc định
            address.IsDefault = model.IsDefault || isFirst;

            if (address.IsDefault)
            {
                await ClearDefaultAsync(userId);
            }

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã thêm địa chỉ mới.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Addresses));
        }

        [HttpGet]
        public async Task<IActionResult> EditAddress(int id)
        {
            var address = await FindAddressAsync(id);

            if (address == null)
            {
                return NotFound();
            }

            return View("AddressForm", new AddressViewModel
            {
                AddressId = address.AddressId,
                ReceiverName = address.ReceiverName,
                Phone = address.Phone,
                Province = address.Province,
                District = address.District,
                Ward = address.Ward,
                AddressDetail = address.AddressDetail,
                IsDefault = address.IsDefault
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(int id, AddressViewModel model)
        {
            var address = await FindAddressAsync(id);

            if (address == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View("AddressForm", model);
            }

            MapAddress(model, address);

            if (model.IsDefault && !address.IsDefault)
            {
                await ClearDefaultAsync(address.UserId);
                address.IsDefault = true;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật địa chỉ.";

            return RedirectToAction(nameof(Addresses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var address = await FindAddressAsync(id);

            if (address != null)
            {
                await ClearDefaultAsync(address.UserId);
                address.IsDefault = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã đặt địa chỉ mặc định.";
            }

            return RedirectToAction(nameof(Addresses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var address = await FindAddressAsync(id);

            if (address == null)
            {
                return RedirectToAction(nameof(Addresses));
            }

            // Địa chỉ đã gắn với đơn hàng thì giữ lại để tra cứu lịch sử
            if (await _context.Orders.AnyAsync(o => o.AddressId == id))
            {
                TempData["Error"] = "Không thể xóa địa chỉ đã được dùng cho đơn hàng.";
                return RedirectToAction(nameof(Addresses));
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            // Nếu xóa địa chỉ mặc định thì chọn địa chỉ khác làm mặc định
            if (address.IsDefault)
            {
                var next = await _context.Addresses
                    .Where(a => a.UserId == address.UserId)
                    .OrderByDescending(a => a.AddressId)
                    .FirstOrDefaultAsync();

                if (next != null)
                {
                    next.IsDefault = true;
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = "Đã xóa địa chỉ.";

            return RedirectToAction(nameof(Addresses));
        }

        // ============================
        // HELPERS
        // ============================
        private Task<Address?> FindAddressAsync(int id)
        {
            int userId = User.GetUserId();

            return _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);
        }

        private async Task ClearDefaultAsync(int userId)
        {
            await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));
        }

        private static void MapAddress(AddressViewModel model, Address address)
        {
            address.ReceiverName = model.ReceiverName.Trim();
            address.Phone = model.Phone.Trim();
            address.Province = model.Province.Trim();
            address.District = model.District.Trim();
            address.Ward = model.Ward.Trim();
            address.AddressDetail = model.AddressDetail.Trim();
        }

        // Cập nhật lại claims (họ tên, email) sau khi sửa hồ sơ
        private async Task RefreshSignInAsync(User user)
        {
            var auth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (auth.Principal?.Identity is not ClaimsIdentity oldIdentity)
            {
                return;
            }

            var claims = oldIdentity.Claims
                .Where(c => c.Type != "FullName" && c.Type != ClaimTypes.Email)
                .Append(new Claim("FullName", user.FullName))
                .Append(new Claim(ClaimTypes.Email, user.Email));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                auth.Properties);
        }
    }
}
