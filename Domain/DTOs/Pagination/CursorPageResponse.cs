namespace Domain.DTOs.Pagination;

public class CursorPageResponse<T>
{
    public List<T> Items { get; set; }
    public string? NextCursor { get; set; }
    public int Count { get; set; }
    public int Limit { get; set; }
    public bool HasMore { get; set; }

    public CursorPageResponse(List<T> items, string? nextCursor, int count, int limit)
    {
        Items = items;
        NextCursor = nextCursor;
        Count = count;
        Limit = limit;
        HasMore = nextCursor != null;
    }
}