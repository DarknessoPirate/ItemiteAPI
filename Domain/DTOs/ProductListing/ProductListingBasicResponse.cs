using Domain.DTOs.Category;

namespace Domain.DTOs.ProductListing;

public class ProductListingBasicResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public bool IsNegotiable { get; set; }
    public string? Location { get; set; }
    public DateTime DateCreated { get; set; }
    public List<CategoryBasicResponse> Categories { get; set; }
    public string? MainImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsArchived { get; set; }
}