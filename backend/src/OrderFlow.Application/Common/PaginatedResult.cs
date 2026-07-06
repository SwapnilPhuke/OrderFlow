namespace OrderFlow.Application.Common;

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    public static PaginatedResult<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize) =>
        new() { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
}
