namespace Domain.Entities;

public class ProductListing : ListingBase
{
    public decimal Price { get; set; }
    public bool IsNegotiable  { get; set; } = false;
}