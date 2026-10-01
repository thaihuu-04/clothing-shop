using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QA.Helpers;
using QA.Models;
using QA.Services;
using QA.ViewModels;

namespace QA.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ClothingStoreDbContext _context;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CheckoutController(
            ClothingStoreDbContext context,
            ICartService cartService,
            IOrderService orderService)
        {
            _context = context;
            _cartService = cartService;
            _orderService = orderService;
        }

        // GET: /Checkout
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await BuildModelAsync(new CheckoutViewModel());

            if (model.Cart.Items.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            if (model.Cart.HasInvalidItems)
            {
                TempData["Error"] = "Một số sản phẩm vượt tồn kho hoặc ngừng bán, vui lòng kiểm tra lại giỏ hàng.";
                return RedirectToAction("Index", "Cart");
            }

            model.AddressId = model.Addresses.FirstOrDefault(a => a.IsDefault)?.AddressId
                ?? model.Addresses.FirstOrDefault()?.AddressId;

            return View(model);
        }

        // POST: /Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel input)
        {
            if (ModelState.IsValid)
            {
                var result = await _orderService.PlaceOrderAsync(
                    User.GetUserId(), input.AddressId!.Value, input.PaymentMethod, input.Note);

                if (result.Success)
                {
                    return RedirectToAction(nameof(Success), new { code = result.Data!.OrderCode });
                }

                ModelState.AddModelError("", result.Message);
            }

            var model = await BuildModelAsync(input);

            if (model.Cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(model);
        }

        // GET: /Checkout/Success?code=DH...
        [HttpGet]
        public async Task<IActionResult> Success(string code)
        {
            int userId = User.GetUserId();

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Address)
                .FirstOrDefaultAsync(o => o.OrderCode == code && o.UserId == userId);

            if (order == null)
            {
                return RedirectToAction("Index", "Orders");
            }

            return View(order);
        }

        private async Task<CheckoutViewModel> BuildModelAsync(CheckoutViewModel model)
        {
            int userId = User.GetUserId();
            var cart = await _cartService.GetCartAsync(userId);

            model.Cart = new CartViewModel
            {
                Items = cart.CartItems.OrderBy(i => i.CartItemId).ToList()
            };

            model.Addresses = await _context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.AddressId)
                .ToListAsync();

            return model;
        }
    }
}
