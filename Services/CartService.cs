using Microsoft.EntityFrameworkCore;
using QA.Helpers;
using QA.Models;

namespace QA.Services
{
    public interface ICartService
    {
        Task<Cart> GetCartAsync(int userId);
        Task<int> CountAsync(int userId);
        Task<ServiceResult> AddAsync(int userId, int productId, int quantity);
        Task<ServiceResult> UpdateQuantityAsync(int userId, int cartItemId, int quantity);
        Task<ServiceResult> RemoveAsync(int userId, int cartItemId);
        Task ClearAsync(int userId);
    }

    public class CartService : ICartService
    {
        private readonly ClothingStoreDbContext _context;

        public CartService(ClothingStoreDbContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng (tạo mới nếu chưa có), đồng bộ lại giá theo giá hiện tại của sản phẩm
        public async Task<Cart> GetCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedAt = DateTime.Now };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                return cart;
            }

            bool changed = false;

            foreach (var item in cart.CartItems)
            {
                if (item.UnitPrice != item.Product.FinalPrice)
                {
                    item.UnitPrice = item.Product.FinalPrice;
                    changed = true;
                }
            }

            if (changed)
            {
                cart.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<int> CountAsync(int userId)
        {
            return await _context.CartItems
                .Where(i => i.Cart.UserId == userId)
                .SumAsync(i => (int?)i.Quantity) ?? 0;
        }

        public async Task<ServiceResult> AddAsync(int userId, int productId, int quantity)
        {
            if (quantity < 1)
            {
                return ServiceResult.Fail("Số lượng không hợp lệ.");
            }

            var product = await _context.Products.FindAsync(productId);

            if (product == null || !product.IsActive)
            {
                return ServiceResult.Fail("Sản phẩm không tồn tại hoặc đã ngừng bán.");
            }

            var cart = await GetCartAsync(userId);

            var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
            int newQuantity = (item?.Quantity ?? 0) + quantity;

            if (newQuantity > product.StockQuantity)
            {
                return ServiceResult.Fail($"Chỉ còn {product.StockQuantity} sản phẩm trong kho.");
            }

            if (newQuantity > ShopSettings.MaxQuantityPerItem)
            {
                return ServiceResult.Fail($"Mỗi sản phẩm chỉ được mua tối đa {ShopSettings.MaxQuantityPerItem}.");
            }

            if (item == null)
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.FinalPrice
                });
            }
            else
            {
                item.Quantity = newQuantity;
                item.UnitPrice = product.FinalPrice;
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return ServiceResult.Ok($"Đã thêm \"{product.ProductName}\" vào giỏ hàng.");
        }

        public async Task<ServiceResult> UpdateQuantityAsync(int userId, int cartItemId, int quantity)
        {
            var item = await FindItemAsync(userId, cartItemId);

            if (item == null)
            {
                return ServiceResult.Fail("Không tìm thấy sản phẩm trong giỏ.");
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                return ServiceResult.Ok("Đã xóa sản phẩm khỏi giỏ hàng.");
            }

            if (quantity > item.Product.StockQuantity)
            {
                return ServiceResult.Fail($"Chỉ còn {item.Product.StockQuantity} sản phẩm \"{item.Product.ProductName}\" trong kho.");
            }

            if (quantity > ShopSettings.MaxQuantityPerItem)
            {
                return ServiceResult.Fail($"Mỗi sản phẩm chỉ được mua tối đa {ShopSettings.MaxQuantityPerItem}.");
            }

            item.Quantity = quantity;
            item.Cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return ServiceResult.Ok("Đã cập nhật giỏ hàng.");
        }

        public async Task<ServiceResult> RemoveAsync(int userId, int cartItemId)
        {
            var item = await FindItemAsync(userId, cartItemId);

            if (item == null)
            {
                return ServiceResult.Fail("Không tìm thấy sản phẩm trong giỏ.");
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return ServiceResult.Ok("Đã xóa sản phẩm khỏi giỏ hàng.");
        }

        public async Task ClearAsync(int userId)
        {
            await _context.CartItems
                .Where(i => i.Cart.UserId == userId)
                .ExecuteDeleteAsync();
        }

        private Task<CartItem?> FindItemAsync(int userId, int cartItemId)
        {
            return _context.CartItems
                .Include(i => i.Product)
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.CartItemId == cartItemId && i.Cart.UserId == userId);
        }
    }
}
