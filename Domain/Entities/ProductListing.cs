namespace Domain.Entities;

public class ProductListing : ListingBase
{
    public decimal Price { get; set; }
    public bool IsNegotiable  { get; set; } = false;
    public bool IsSold { get; set; } = false;
    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }
}