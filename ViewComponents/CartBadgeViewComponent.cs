using Microsoft.AspNetCore.Mvc;
using QA.Helpers;
using QA.Services;

namespace QA.ViewComponents
{
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartBadgeViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;

            if (UserClaimsPrincipal.Identity?.IsAuthenticated == true)
            {
                count = await _cartService.CountAsync(UserClaimsPrincipal.GetUserId());
            }

            return View(count);
        }
    }
}
