namespace Domain.Entities;

public class ProductListing : ListingBase
{
    public int Price { get; set; }
    public bool IsNegotiable  { get; set; } = false;
}