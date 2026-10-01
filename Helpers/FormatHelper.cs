using System.Globalization;

namespace QA.Helpers
{
    public static class FormatHelper
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        // 199000 => "199.000đ"
        public static string Money(decimal? amount)
        {
            return (amount ?? 0).ToString("N0", Vi) + "đ";
        }

        public static string DateTime(DateTime? value)
        {
            return value?.ToString("dd/MM/yyyy HH:mm") ?? "";
        }
    }
}
