using System.ComponentModel.DataAnnotations;
using QA.Models;

namespace QA.Areas.Admin.Models
{
    // ============================
    // DASHBOARD
    // ============================
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ActiveProducts { get; set; }

        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public List<string> RevenueLabels { get; set; } = new();
        public List<decimal> RevenueValues { get; set; } = new();

        public List<Order> RecentOrders { get; set; } = new();
        public List<TopProductItem> TopProducts { get; set; } = new();
        public List<Product> LowStockProducts { get; set; } = new();
    }

    public class TopProductItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    // ============================
    // PRODUCT
    // ============================
    public class ProductFormViewModel : IValidatableObject
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200)]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }

        [Display(Name = "Thương hiệu")]
        public int? BrandId { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(1000, 1_000_000_000, ErrorMessage = "Giá phải từ 1.000đ")]
        [Display(Name = "Giá gốc (đ)")]
        public decimal? Price { get; set; }

        [Range(0, 1_000_000_000, ErrorMessage = "Giá khuyến mãi không hợp lệ")]
        [Display(Name = "Giá khuyến mãi (đ)")]
        public decimal? SalePrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tồn kho")]
        [Range(0, 100000, ErrorMessage = "Tồn kho không hợp lệ")]
        [Display(Name = "Tồn kho")]
        public int? StockQuantity { get; set; }

        [StringLength(50)]
        [Display(Name = "Kích cỡ (phân cách bằng dấu phẩy)")]
        public string? Size { get; set; }

        [StringLength(50)]
        [Display(Name = "Màu sắc")]
        public string? Color { get; set; }

        [StringLength(100)]
        [Display(Name = "Chất liệu")]
        public string? Material { get; set; }

        [StringLength(500)]
        [Display(Name = "Link ảnh (hoặc tải ảnh bên dưới)")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Tải ảnh lên")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Sản phẩm nổi bật")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Đang bán")]
        public bool IsActive { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SalePrice.HasValue && SalePrice > 0 && Price.HasValue && SalePrice >= Price)
            {
                yield return new ValidationResult(
                    "Giá khuyến mãi phải nhỏ hơn giá gốc",
                    new[] { nameof(SalePrice) });
            }
        }
    }

    // ============================
    // CATEGORY / BRAND
    // ============================
    public class CategoryFormViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100)]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; } = "";

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Link ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Tải ảnh lên")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Hiển thị")]
        public bool IsActive { get; set; } = true;
    }

    public class BrandFormViewModel
    {
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thương hiệu")]
        [StringLength(100)]
        [Display(Name = "Tên thương hiệu")]
        public string BrandName { get; set; } = "";

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Link logo")]
        public string? LogoUrl { get; set; }

        [Display(Name = "Tải logo lên")]
        public IFormFile? LogoFile { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;
    }

    // ============================
    // USERS
    // ============================
    public class UserListItem
    {
        public User User { get; set; } = null!;
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
