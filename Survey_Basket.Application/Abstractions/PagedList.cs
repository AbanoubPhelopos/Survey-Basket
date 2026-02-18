using System.Text.Json.Serialization;

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

    // Parameterless constructor for JSON deserialization
    [JsonConstructor]
    public PagedList()
    {
        Items = [];
    }

    public IEnumerable<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
