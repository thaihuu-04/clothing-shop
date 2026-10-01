namespace QA.Helpers
{
    // Các giá trị khớp với default trong database
    public static class OrderStatus
    {
        public const string Pending = "Chờ xác nhận";
        public const string Confirmed = "Đã xác nhận";
        public const string Shipping = "Đang giao";
        public const string Delivered = "Đã giao";
        public const string Cancelled = "Đã hủy";

        public static readonly string[] All =
        {
            Pending, Confirmed, Shipping, Delivered, Cancelled
        };

        // Trạng thái tiếp theo hợp lệ (dùng cho admin)
        public static string[] NextStatuses(string current) => current switch
        {
            Pending => new[] { Confirmed, Cancelled },
            Confirmed => new[] { Shipping, Cancelled },
            Shipping => new[] { Delivered, Cancelled },
            _ => Array.Empty<string>()
        };

        public static string BadgeClass(string status) => status switch
        {
            Pending => "bg-warning text-dark",
            Confirmed => "bg-info text-dark",
            Shipping => "bg-primary",
            Delivered => "bg-success",
            Cancelled => "bg-secondary",
            _ => "bg-light text-dark"
        };
    }

    public static class PaymentStatus
    {
        public const string Unpaid = "Chưa thanh toán";
        public const string Paid = "Đã thanh toán";
        public const string Refunded = "Đã hoàn tiền";
    }

    public static class PaymentMethods
    {
        public const string Cod = "COD";
        public const string BankTransfer = "Chuyển khoản";

        public static readonly string[] All = { Cod, BankTransfer };

        public static string DisplayName(string method) => method switch
        {
            Cod => "Thanh toán khi nhận hàng (COD)",
            BankTransfer => "Chuyển khoản ngân hàng",
            _ => method
        };
    }
}
