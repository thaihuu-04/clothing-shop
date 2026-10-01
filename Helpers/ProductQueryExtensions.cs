using QA.Models;

namespace QA.Helpers
{
    // Các truy vấn dùng chung cho Product (dịch được sang SQL)
    public static class ProductQueryExtensions
    {
        public static IQueryable<Product> OnlyVisible(this IQueryable<Product> query)
        {
            return query.Where(p => p.IsActive && p.Category.IsActive);
        }

        public static IQueryable<Product> WhereOnSale(this IQueryable<Product> query)
        {
            return query.Where(p => p.SalePrice != null && p.SalePrice > 0 && p.SalePrice < p.Price);
        }

        public static IQueryable<Product> WherePriceFrom(this IQueryable<Product> query, decimal min)
        {
            return query.Where(p =>
                (p.SalePrice != null && p.SalePrice > 0 && p.SalePrice < p.Price ? p.SalePrice.Value : p.Price) >= min);
        }

        public static IQueryable<Product> WherePriceTo(this IQueryable<Product> query, decimal max)
        {
            return query.Where(p =>
                (p.SalePrice != null && p.SalePrice > 0 && p.SalePrice < p.Price ? p.SalePrice.Value : p.Price) <= max);
        }

        public static IQueryable<Product> ApplySort(this IQueryable<Product> query, string? sort)
        {
            return sort switch
            {
                "price_asc" => query.OrderBy(p =>
                    p.SalePrice != null && p.SalePrice > 0 && p.SalePrice < p.Price ? p.SalePrice.Value : p.Price),
                "price_desc" => query.OrderByDescending(p =>
                    p.SalePrice != null && p.SalePrice > 0 && p.SalePrice < p.Price ? p.SalePrice.Value : p.Price),
                "name" => query.OrderBy(p => p.ProductName),
                _ => query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.ProductId)
            };
        }
    }
}
