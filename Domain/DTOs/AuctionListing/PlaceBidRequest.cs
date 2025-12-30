namespace Domain.DTOs.AuctionListing;

public class PlaceBidRequest
{
    public decimal Price { get; set; }
    public string PaymentMethodId { get; set; }
}