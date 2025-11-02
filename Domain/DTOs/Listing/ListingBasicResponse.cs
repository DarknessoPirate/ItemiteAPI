using Domain.DTOs.Category;
using Domain.DTOs.ProductListing;
using Domain.ValueObjects;

namespace Domain.DTOs.Listing;

public class ListingBasicResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public Location Location { get; set; }
    public DateTime DateCreated { get; set; }
    public List<CategoryBasicResponse> Categories { get; set; }
    public string? MainImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsArchived { get; set; }
    public int Followers { get; set; }
    
    public string ListingType { get; set; }
    
    // Product listing
    public decimal? Price { get; set; }
    public bool? IsNegotiable { get; set; }
    
    // Auction listing
    public decimal? StartingBid { get; set; }
    public decimal? CurrentBid { get; set; }
}