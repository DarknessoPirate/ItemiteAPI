using Domain.DTOs.Category;
using Domain.DTOs.User;

namespace Domain.DTOs.ProductListing;

public class ProductListingResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int Views { get; set; }
    public DateTime DateCreated { get; set; }
    public bool IsArchived { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNegotiable { get; set; }
    public UserResponse? Owner { get; set; }
    public string? Location { get; set; }
    public List<CategoryResponse>? Categories { get; set; }
    public List<string>? ImagesUrls { get; set; }
    public string? MainImageUrl { get; set; }
}