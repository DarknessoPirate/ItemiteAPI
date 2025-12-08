using Domain.DTOs.Listing;
using Domain.DTOs.User;
using Domain.Enums;

namespace Domain.DTOs.Payments;

public class PaymentSellerResponse
{
    public int PaymentId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PlatformFeeAmount { get; set; }
    public decimal SellerAmount { get; set; }
    public string Currency { get; set; }
    public ListingBasicResponse Listing { get; set; } = null!;
    public UserBasicResponse Buyer { get; set; } = null!;
    public UserBasicResponse Seller { get; set; } = null!;
    public PaymentStatus Status { get; set; }
    public DateTime ChargeDate { get; set; }
    public DateTime? TransferDate { get; set; }
    public DateTime? ScheduledTransferDate { get; set; }
}