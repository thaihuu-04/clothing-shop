namespace QA.ViewModels
{
    public class PagerViewModel
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }

        // Tạo route values (gồm cả các tham số lọc) cho một trang cụ thể
        public Func<int, Dictionary<string, string>> RouteValuesFor { get; set; } =
            p => new Dictionary<string, string> { ["page"] = p.ToString() };
    }
}
