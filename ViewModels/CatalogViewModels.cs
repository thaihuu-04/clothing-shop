using QA.Helpers;
using QA.Models;

namespace QA.ViewModels
{
    public class ProductFilter
    {
        public string? Q { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool OnSale { get; set; }
        public string? Sort { get; set; }
        public int Page { get; set; } = 1;

        // Giữ lại các tham số lọc khi chuyển trang
        public Dictionary<string, string> ToRouteValues(int? page = null)
        {
            var values = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(Q)) values["q"] = Q;
            if (CategoryId.HasValue) values["categoryId"] = CategoryId.Value.ToString();
            if (BrandId.HasValue) values["brandId"] = BrandId.Value.ToString();
            if (MinPrice.HasValue) values["minPrice"] = MinPrice.Value.ToString("0");
            if (MaxPrice.HasValue) values["maxPrice"] = MaxPrice.Value.ToString("0");
            if (OnSale) values["onSale"] = "true";
            if (!string.IsNullOrWhiteSpace(Sort)) values["sort"] = Sort;
            values["page"] = (page ?? Page).ToString();

            return values;
        }
    }

    public class ProductListViewModel
    {
        public PagedList<Product> Products { get; set; } = null!;
        public ProductFilter Filter { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Brand> Brands { get; set; } = new();

        public static readonly Dictionary<string, string> SortOptions = new()
        {
            ["newest"] = "Mới nhất",
            ["price_asc"] = "Giá tăng dần",
            ["price_desc"] = "Giá giảm dần",
            ["name"] = "Tên A-Z"
        };
    }

    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;
        public List<Product> RelatedProducts { get; set; } = new();
    }

    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<Product> FeaturedProducts { get; set; } = new();
        public List<Product> NewProducts { get; set; } = new();
        public List<Product> SaleProducts { get; set; } = new();
    }
}
