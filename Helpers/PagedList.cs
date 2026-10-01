using Microsoft.EntityFrameworkCore;

namespace QA.Helpers
{
    public class PagedList<T>
    {
        public List<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        public PagedList(List<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }

        public static async Task<PagedList<T>> CreateAsync(
            IQueryable<T> source, int page, int pageSize)
        {
            int count = await source.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));

            page = Math.Clamp(page, 1, totalPages);

            var items = await source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<T>(items, count, page, pageSize);
        }
    }
}
