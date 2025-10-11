namespace Domain.DTOs.Pagination;

public class PageResponse<T>
{
    public List<T> Items { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int ItemsFrom { get; set; }
    public int ItemsTo { get; set; }
    public int TotalItemsCount { get; set; }
    public bool HasNextPage { get; set; }

    public PageResponse() { }
    public PageResponse(List<T> items, int totalCount, int pageSize, int pageNumber)
    {
        Items = items;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        CurrentPage = pageNumber;
        ItemsFrom = pageSize * (pageNumber - 1) + 1;
        ItemsTo = ItemsFrom + pageSize - 1;
        TotalItemsCount = totalCount;
        HasNextPage =  pageNumber < TotalPages;
    }
}