using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Models;

// Phần mở rộng của entity sinh tự động (không sửa Product.cs để tránh mất khi scaffold lại)
public partial class Product
{
    // Giá bán thực tế (ưu tiên giá khuyến mãi)
    [NotMapped]
    public decimal FinalPrice =>
        SalePrice.HasValue && SalePrice.Value > 0 && SalePrice.Value < Price
            ? SalePrice.Value
            : Price;

    [NotMapped]
    public bool IsOnSale => FinalPrice < Price;

    [NotMapped]
    public int DiscountPercent =>
        IsOnSale ? (int)Math.Round((Price - FinalPrice) * 100 / Price) : 0;

    [NotMapped]
    public string DisplayImage =>
        string.IsNullOrWhiteSpace(ImageUrl) ? "/images/no-image.svg" : ImageUrl;

    public IEnumerable<string> GetSizes() =>
        (Size ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
