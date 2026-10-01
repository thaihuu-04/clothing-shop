using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QA.Helpers;
using QA.Models;
using QA.Services;

namespace QA.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private const int PageSize = 10;

        private readonly ClothingStoreDbContext _context;
        private readonly IOrderService _orderService;

        public OrdersController(ClothingStoreDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        // GET: /Orders?status=&page=
        public async Task<IActionResult> Index(string? status, int page = 1)
        {
            int userId = User.GetUserId();

            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Where(o => o.UserId == userId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            ViewBag.Status = status;

            var orders = await PagedList<Order>.CreateAsync(
                query.OrderByDescending(o => o.CreatedAt), page, PageSize);

            return View(orders);
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            int userId = User.GetUserId();

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Address)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: /Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _orderService.CancelByUserAsync(User.GetUserId(), id);

            TempData[result.Success ? "Success" : "Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
