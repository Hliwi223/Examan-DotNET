namespace Esport.Application.Common;

/// <summary>
/// Param\u00e8tres de pagination utilis\u00e9s par les requ\u00eates de lecture (Queries).
/// </summary>
public class PaginationParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int Skip => (Math.Max(Page, 1) - 1) * Math.Min(PageSize, 50);
    public int Take => Math.Min(Math.Max(PageSize, 1), 50);
}

/// <summary>
/// R\u00e9sultat pagin\u00e9 g\u00e9n\u00e9rique renvoy\u00e9 par les Queries de liste.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
