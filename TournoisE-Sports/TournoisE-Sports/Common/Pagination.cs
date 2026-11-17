using System.Text.Json.Serialization;

namespace TournoisE_Sports.Common;

public class PaginationParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    [JsonIgnore]
    public int Skip => (Math.Max(Page, 1) - 1) * Math.Min(PageSize, 50);

    [JsonIgnore]
    public int Take => Math.Min(Math.Max(PageSize, 1), 50);
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
