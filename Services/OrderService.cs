using Microsoft.EntityFrameworkCore;
using QA.Helpers;
using QA.Models;

namespace QA.Services
{
    public interface IOrderService
    {
        Task<ServiceResult<Order>> PlaceOrderAsync(int userId, int addressId, string paymentMethod, string? note);
        Task<ServiceResult> CancelByUserAsync(int userId, int orderId);
        Task<ServiceResult> UpdateStatusAsync(int orderId, string newStatus);
        Task<ServiceResult> UpdatePaymentStatusAsync(int orderId, string paymentStatus);
    }

    public class OrderService : IOrderService
    {
        private readonly ClothingStoreDbContext _context;

        public OrderService(ClothingStoreDbContext context)
        {
            _context = context;
        }

        // ============================
        // ĐẶT HÀNG
        // ============================
        public async Task<ServiceResult<Order>> PlaceOrderAsync(
            int userId, int addressId, string paymentMethod, string? note)
        {
            if (!PaymentMethods.All.Contains(paymentMethod))
            {
                return ServiceResult<Order>.Fail("Phương thức thanh toán không hợp lệ.");
            }

            bool addressValid = await _context.Addresses
                .AnyAsync(a => a.AddressId == addressId && a.UserId == userId);

            if (!addressValid)
            {
                return ServiceResult<Order>.Fail("Vui lòng chọn địa chỉ giao hàng hợp lệ.");
            }

            var cartItems = await _context.CartItems
                .Include(i => i.Product)
                .Where(i => i.Cart.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return ServiceResult<Order>.Fail("Giỏ hàng của bạn đang trống.");
            }

            var inactive = cartItems.FirstOrDefault(i => !i.Product.IsActive);
            if (inactive != null)
            {
                return ServiceResult<Order>.Fail($"Sản phẩm \"{inactive.Product.ProductName}\" đã ngừng bán, vui lòng xóa khỏi giỏ.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Trừ kho có điều kiện để tránh bán vượt tồn kho khi nhiều người đặt cùng lúc
                foreach (var item in cartItems)
                {
                    int affected = await _context.Products
                        .Where(p => p.ProductId == item.ProductId && p.StockQuantity >= item.Quantity)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.StockQuantity, p => p.StockQuantity - item.Quantity)
                            .SetProperty(p => p.UpdatedAt, DateTime.Now));

                    if (affected == 0)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<Order>.Fail(
                            $"Sản phẩm \"{item.Product.ProductName}\" không đủ số lượng trong kho.");
                    }
                }

                decimal subTotal = cartItems.Sum(i => i.Product.FinalPrice * i.Quantity);
                decimal total = subTotal + ShopSettings.CalculateShipping(subTotal);

                var order = new Order
                {
                    UserId = userId,
                    AddressId = addressId,
                    OrderCode = await GenerateOrderCodeAsync(),
                    TotalAmount = total,
                    OrderStatus = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Unpaid,
                    PaymentMethod = paymentMethod,
                    Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                    CreatedAt = DateTime.Now,
                    Payment = new Payment
                    {
                        Amount = total,
                        PaymentMethod = paymentMethod,
                        PaymentStatus = PaymentStatus.Unpaid,
                        CreatedAt = DateTime.Now
                    }
                };

                foreach (var item in cartItems)
                {
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.FinalPrice
                    });
                }

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult<Order>.Ok(order, "Đặt hàng thành công!");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ============================
        // KHÁCH HỦY ĐƠN
        // ============================
        public async Task<ServiceResult> CancelByUserAsync(int userId, int orderId)
        {
            var order = await LoadOrderAsync(orderId);

            if (order == null || order.UserId != userId)
            {
                return ServiceResult.Fail("Không tìm thấy đơn hàng.");
            }

            if (order.OrderStatus != OrderStatus.Pending)
            {
                return ServiceResult.Fail("Chỉ có thể hủy đơn hàng đang chờ xác nhận.");
            }

            await CancelAsync(order);

            return ServiceResult.Ok($"Đã hủy đơn hàng {order.OrderCode}.");
        }

        // ============================
        // ADMIN CẬP NHẬT TRẠNG THÁI
        // ============================
        public async Task<ServiceResult> UpdateStatusAsync(int orderId, string newStatus)
        {
            var order = await LoadOrderAsync(orderId);

            if (order == null)
            {
                return ServiceResult.Fail("Không tìm thấy đơn hàng.");
            }

            if (!OrderStatus.NextStatuses(order.OrderStatus).Contains(newStatus))
            {
                return ServiceResult.Fail($"Không thể chuyển từ \"{order.OrderStatus}\" sang \"{newStatus}\".");
            }

            if (newStatus == OrderStatus.Cancelled)
            {
                await CancelAsync(order);
                return ServiceResult.Ok($"Đã hủy đơn hàng {order.OrderCode} và hoàn lại tồn kho.");
            }

            order.OrderStatus = newStatus;
            order.UpdatedAt = DateTime.Now;

            // Giao thành công đơn COD => coi như đã thu tiền
            if (newStatus == OrderStatus.Delivered && order.PaymentMethod == PaymentMethods.Cod)
            {
                SetPaymentStatus(order, PaymentStatus.Paid);
            }

            await _context.SaveChangesAsync();

            return ServiceResult.Ok($"Đơn hàng {order.OrderCode} chuyển sang \"{newStatus}\".");
        }

        public async Task<ServiceResult> UpdatePaymentStatusAsync(int orderId, string paymentStatus)
        {
            var allowed = new[] { PaymentStatus.Unpaid, PaymentStatus.Paid, PaymentStatus.Refunded };

            if (!allowed.Contains(paymentStatus))
            {
                return ServiceResult.Fail("Trạng thái thanh toán không hợp lệ.");
            }

            var order = await LoadOrderAsync(orderId);

            if (order == null)
            {
                return ServiceResult.Fail("Không tìm thấy đơn hàng.");
            }

            SetPaymentStatus(order, paymentStatus);
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return ServiceResult.Ok("Đã cập nhật trạng thái thanh toán.");
        }

        // ============================
        // HELPERS
        // ============================
        private Task<Order?> LoadOrderAsync(int orderId)
        {
            return _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        private async Task CancelAsync(Order order)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var detail in order.OrderDetails)
            {
                await _context.Products
                    .Where(p => p.ProductId == detail.ProductId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.StockQuantity, p => p.StockQuantity + detail.Quantity));
            }

            order.OrderStatus = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.Now;

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                SetPaymentStatus(order, PaymentStatus.Refunded);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private static void SetPaymentStatus(Order order, string status)
        {
            order.PaymentStatus = status;

            if (order.Payment != null)
            {
                order.Payment.PaymentStatus = status;
                order.Payment.PaidAt = status == PaymentStatus.Paid ? DateTime.Now : order.Payment.PaidAt;
            }
        }

        private async Task<string> GenerateOrderCodeAsync()
        {
            string code;

            do
            {
                code = $"DH{DateTime.Now:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";
            }
            while (await _context.Orders.AnyAsync(o => o.OrderCode == code));

            return code;
        }
    }
}
