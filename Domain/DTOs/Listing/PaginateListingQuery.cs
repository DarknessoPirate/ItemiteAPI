using Domain.Enums;

namespace Domain.DTOs.Listing;

public class PaginateListingQuery
{
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public ListingType? ListingType { get; set; }
    public SortBy? SortBy { get; set; } = Domain.Enums.SortBy.CreationDate;
    public SortDirection? SortDirection { get; set; } = Domain.Enums.SortDirection.Ascending;
    public bool? FollowedOnly { get; set; } = false;
    public decimal? PriceFrom { get; set; } 
    public decimal? PriceTo { get; set; }
    public List<int>? CategoryIds { get; set; }

    public override string ToString()
    {
        var categoriesString = CategoryIds != null && CategoryIds.Any() 
            ? string.Join("-", CategoryIds) 
            : "null";
    
        return $"{PageSize.ToString()}_" +
               $"{PageNumber.ToString()}_" +
               $"{ListingType?.ToString() ?? "null"}_" +
               $"{SortBy?.ToString() ?? "null"}_" +
               $"{SortDirection?.ToString() ?? "null"}_" +
               $"{PriceFrom?.ToString() ?? "null"}_" +
               $"{PriceTo?.ToString() ?? "null"}_" +
               $"{FollowedOnly?.ToString() ?? "null"}_" +
               $"{categoriesString}";
    }
}