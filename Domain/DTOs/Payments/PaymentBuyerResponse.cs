using Domain.DTOs.Listing;
using Domain.DTOs.User;
using Domain.Enums;

namespace Domain.DTOs.Payments;

public class PaymentBuyerResponse
{
    public int PaymentId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; }
    public ListingBasicResponse Listing { get; set; } = null!;
    public UserBasicResponse Buyer { get; set; } = null!;
    public UserBasicResponse Seller { get; set; } = null!;
    public PaymentStatus Status { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundDate { get; set; }
    public DisputeUserResponse? Dispute { get; set; }
}