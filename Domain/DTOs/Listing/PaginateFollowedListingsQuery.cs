namespace Domain.DTOs.Listing;

public class PaginateFollowedListingsQuery
{
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    
    public override string ToString()
    {
        return "followed_" +
               $"{PageSize.ToString()}_" +
               $"{PageNumber.ToString()}";
    }
}