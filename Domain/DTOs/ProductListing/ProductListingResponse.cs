using Domain.DTOs.Category;
using Domain.DTOs.Listing;
using Domain.DTOs.User;
using Domain.ValueObjects;

namespace Domain.DTOs.ProductListing;

public class ProductListingResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? YourPrice { get; set; }
    public int Views { get; set; }
    public int Followers { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateEnds { get; set; }
    public bool IsArchived { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNegotiable { get; set; }
    public bool? IsFollowed { get; set; }
    public UserResponse? Owner { get; set; }
    public Location Location { get; set; }
    public List<CategoryResponse>? Categories { get; set; }
    public List<ListingImageResponse>? Images { get; set; }
    public string? MainImageUrl { get; set; }
}