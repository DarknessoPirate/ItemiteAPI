namespace Domain.DTOs.Listing;

public class PaginateUserListingsQuery
{
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public bool? AreArchived { get; set; }
    
    public override string ToString()
    {
        return "user_" +
               $"{PageSize.ToString()}_" +
               $"{PageNumber.ToString()}_"+
               $"{AreArchived?.ToString() ?? "null"}";
    }
}