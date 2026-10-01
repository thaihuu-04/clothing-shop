using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using QA.Helpers;
using QA.Models;

namespace QA.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();

        public decimal SubTotal => Items.Sum(i => i.UnitPrice * i.Quantity);
        public decimal ShippingFee => ShopSettings.CalculateShipping(SubTotal);
        public decimal Total => SubTotal + ShippingFee;
        public int TotalQuantity => Items.Sum(i => i.Quantity);

        public decimal AmountToFreeShipping =>
            SubTotal > 0 && SubTotal < ShopSettings.FreeShippingThreshold
                ? ShopSettings.FreeShippingThreshold - SubTotal
                : 0;

        // Có sản phẩm nào vượt tồn kho / ngừng bán không
        public bool HasInvalidItems =>
            Items.Any(i => !i.Product.IsActive || i.Quantity > i.Product.StockQuantity);
    }

    public class CheckoutViewModel
    {
        [ValidateNever]
        public CartViewModel Cart { get; set; } = new();

        [ValidateNever]
        public List<Address> Addresses { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng chọn địa chỉ giao hàng")]
        public int? AddressId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = PaymentMethods.Cod;

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string? Note { get; set; }
    }
}
