using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QA.Areas.Admin.Models;
using QA.Helpers;
using QA.Models;

namespace QA.Areas.Admin.Controllers
{
    public class UsersController : AdminControllerBase
    {
        private const int PageSize = 15;

        private readonly ClothingStoreDbContext _context;

        public UsersController(ClothingStoreDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Users?q=&roleId=&page=
        public async Task<IActionResult> Index(string? q, int? roleId, int page = 1)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                string keyword = q.Trim();
                query = query.Where(u =>
                    u.Username.Contains(keyword) ||
                    u.FullName.Contains(keyword) ||
                    u.Email.Contains(keyword) ||
                    (u.Phone != null && u.Phone.Contains(keyword)));
            }

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId);
            }

            var projected = query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserListItem
                {
                    User = u,
                    OrderCount = u.Orders.Count(),
                    TotalSpent = u.Orders
                        .Where(o => o.PaymentStatus == PaymentStatus.Paid)
                        .Sum(o => (decimal?)o.TotalAmount) ?? 0
                });

            var roles = await _context.Roles.AsNoTracking().ToListAsync();

            ViewBag.Q = q;
            ViewBag.RoleId = roleId;
            ViewBag.Roles = roles;
            ViewBag.RoleFilter = new SelectList(roles, "RoleId", "RoleName", roleId);
            ViewBag.CurrentUserId = User.GetUserId();
            ViewData["Title"] = "Người dùng";

            return View(await PagedList<UserListItem>.CreateAsync(projected, page, PageSize));
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Addresses)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = await _context.Roles.AsNoTracking().ToListAsync();
            ViewBag.CurrentUserId = User.GetUserId();
            ViewData["Title"] = user.FullName;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, string? returnUrl)
        {
            if (id == User.GetUserId())
            {
                Error("Bạn không thể khóa tài khoản của chính mình.");
                return RedirectBack(returnUrl);
            }

            var user = await _context.Users.FindAsync(id);

            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                Success($"Đã {(user.IsActive ? "mở khóa" : "khóa")} tài khoản \"{user.Username}\".");
            }

            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, int roleId, string? returnUrl)
        {
            if (id == User.GetUserId())
            {
                Error("Bạn không thể đổi quyền của chính mình.");
                return RedirectBack(returnUrl);
            }

            var user = await _context.Users.FindAsync(id);
            var role = await _context.Roles.FindAsync(roleId);

            if (user != null && role != null)
            {
                user.RoleId = roleId;
                await _context.SaveChangesAsync();

                Success($"Đã đổi quyền của \"{user.Username}\" thành {role.RoleName}. Người dùng cần đăng nhập lại để áp dụng.");
            }

            return RedirectBack(returnUrl);
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
