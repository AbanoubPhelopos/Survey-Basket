namespace Survey_Basket.Application.Abstractions;

public class PagedList<T>
{
    public PagedList(IEnumerable<T> items, int pageNumber, int totalCount, int pageSize)
    {
        Items = items;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        TotalCount = totalCount;
    }

    public IEnumerable<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
