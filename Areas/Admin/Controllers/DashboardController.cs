using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QA.Areas.Admin.Models;
using QA.Helpers;
using QA.Models;

namespace QA.Areas.Admin.Controllers
{
    public class DashboardController : AdminControllerBase
    {
        private const int CustomerRoleId = 2;

        private readonly ClothingStoreDbContext _context;

        public DashboardController(ClothingStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfChart = startOfMonth.AddMonths(-11);

            // Doanh thu = các đơn đã thanh toán (COD giao thành công hoặc đã nhận chuyển khoản)
            var paidOrders = _context.Orders.Where(o => o.PaymentStatus == PaymentStatus.Paid);

            var model = new DashboardViewModel
            {
                TotalRevenue = await paidOrders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                RevenueThisMonth = await paidOrders
                    .Where(o => o.CreatedAt >= startOfMonth)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                TotalOrders = await _context.Orders.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Pending),
                TotalCustomers = await _context.Users.CountAsync(u => u.RoleId == CustomerRoleId),
                NewCustomersThisMonth = await _context.Users
                    .CountAsync(u => u.RoleId == CustomerRoleId && u.CreatedAt >= startOfMonth),
                ActiveProducts = await _context.Products.CountAsync(p => p.IsActive)
            };

            // Đơn hàng theo trạng thái
            var statusCounts = await _context.Orders
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            model.OrdersByStatus = OrderStatus.All.ToDictionary(
                s => s,
                s => statusCounts.FirstOrDefault(x => x.Status == s)?.Count ?? 0);

            // Doanh thu 12 tháng gần nhất
            var monthly = await paidOrders
                .Where(o => o.CreatedAt >= startOfChart)
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => o.TotalAmount) })
                .ToListAsync();

            for (int i = 0; i < 12; i++)
            {
                var month = startOfChart.AddMonths(i);
                model.RevenueLabels.Add(month.ToString("MM/yyyy"));
                model.RevenueValues.Add(monthly
                    .FirstOrDefault(m => m.Year == month.Year && m.Month == month.Month)?.Total ?? 0);
            }

            model.RecentOrders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(6)
                .ToListAsync();

            model.TopProducts = await _context.OrderDetails
                .Where(d => d.Order.OrderStatus != OrderStatus.Cancelled)
                .GroupBy(d => new { d.ProductId, d.Product.ProductName, d.Product.ImageUrl })
                .Select(g => new TopProductItem
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    ImageUrl = g.Key.ImageUrl,
                    QuantitySold = g.Sum(d => d.Quantity),
                    Revenue = g.Sum(d => d.Quantity * d.UnitPrice)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            model.LowStockProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive && p.StockQuantity <= ShopSettings.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .Take(8)
                .ToListAsync();

            ViewData["Title"] = "Dashboard";

            return View(model);
        }
    }
}
