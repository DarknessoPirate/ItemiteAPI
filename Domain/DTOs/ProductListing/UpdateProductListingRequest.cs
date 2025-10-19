using Domain.DTOs.File;

namespace Domain.DTOs.ProductListing;

public class UpdateProductListingRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public decimal Price { get; set; }
    public bool? IsNegotiable { get; set; } = false;
    public int CategoryId { get; set; }
    
    public List<int>? ExistingPhotoIds { get; set; }
    public List<int>? ExistingPhotoOrders { get; set; }
}