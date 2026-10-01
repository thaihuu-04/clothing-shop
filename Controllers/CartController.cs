using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QA.Helpers;
using QA.Services;
using QA.ViewModels;

namespace QA.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync(User.GetUserId());

            return View(new CartViewModel
            {
                Items = cart.CartItems.OrderBy(i => i.CartItemId).ToList()
            });
        }

        // POST: /Cart/Add (hỗ trợ cả AJAX lẫn form thường)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            int userId = User.GetUserId();
            var result = await _cartService.AddAsync(userId, productId, quantity);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    count = await _cartService.CountAsync(userId)
                });
            }

            SetMessage(result);

            return RedirectBack();
        }

        // POST: /Cart/BuyNow => thêm vào giỏ rồi chuyển sang thanh toán
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNow(int productId, int quantity = 1)
        {
            var result = await _cartService.AddAsync(User.GetUserId(), productId, quantity);

            if (!result.Success)
            {
                SetMessage(result);
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            return RedirectToAction("Index", "Checkout");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int cartItemId, int quantity)
        {
            var result = await _cartService.UpdateQuantityAsync(User.GetUserId(), cartItemId, quantity);

            if (!result.Success)
            {
                SetMessage(result);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            SetMessage(await _cartService.RemoveAsync(User.GetUserId(), cartItemId));

            return RedirectToAction(nameof(Index));
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers.XRequestedWith == "XMLHttpRequest";
        }

        private void SetMessage(ServiceResult result)
        {
            TempData[result.Success ? "Success" : "Error"] = result.Message;
        }

        private IActionResult RedirectBack()
        {
            string? referer = Request.Headers.Referer.ToString();

            if (!string.IsNullOrEmpty(referer) &&
                Uri.TryCreate(referer, UriKind.Absolute, out var uri) &&
                Url.IsLocalUrl(uri.PathAndQuery) &&
                uri.Host == Request.Host.Host)
            {
                return LocalRedirect(uri.PathAndQuery);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
