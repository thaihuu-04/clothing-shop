using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QA.Helpers;
using QA.Models;
using QA.Services;

namespace QA.Areas.Admin.Controllers
{
    public class OrdersController : AdminControllerBase
    {
        private const int PageSize = 15;

        private readonly ClothingStoreDbContext _context;
        private readonly IOrderService _orderService;

        public OrdersController(ClothingStoreDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        // GET: /Admin/Orders?status=&q=&from=&to=&page=
        public async Task<IActionResult> Index(string? status, string? q, DateTime? from, DateTime? to, int page = 1)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Address)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                string keyword = q.Trim();
                query = query.Where(o =>
                    o.OrderCode.Contains(keyword) ||
                    o.User.FullName.Contains(keyword) ||
                    o.User.Username.Contains(keyword) ||
                    (o.Address != null && o.Address.Phone.Contains(keyword)));
            }

            if (from.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(o => o.CreatedAt < to.Value.Date.AddDays(1));
            }

            ViewBag.Status = status;
            ViewBag.Q = q;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.StatusCounts = await _context.Orders
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
            ViewData["Title"] = "Quản lý đơn hàng";

            var orders = await PagedList<Order>.CreateAsync(
                query.OrderByDescending(o => o.CreatedAt), page, PageSize);

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Address)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Đơn hàng " + order.OrderCode;

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            SetMessage(await _orderService.UpdateStatusAsync(id, status));

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePayment(int id, string paymentStatus)
        {
            SetMessage(await _orderService.UpdatePaymentStatusAsync(id, paymentStatus));

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
