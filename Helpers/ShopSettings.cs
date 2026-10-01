namespace QA.Helpers
{
    public static class ShopSettings
    {
        // Miễn phí vận chuyển cho đơn từ mức này
        public const decimal FreeShippingThreshold = 500000;

        public const decimal ShippingFee = 30000;

        public const int MaxQuantityPerItem = 10;

        public const int LowStockThreshold = 5;

        public static decimal CalculateShipping(decimal subTotal)
        {
            return subTotal == 0 || subTotal >= FreeShippingThreshold ? 0 : ShippingFee;
        }
    }
}
