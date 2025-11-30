using System.ComponentModel.DataAnnotations;
using Domain.DTOs.Listing;
using Domain.DTOs.User;
using Domain.Enums;

namespace Domain.DTOs.Payments;

public class PaymentResponse
{
    public int PaymentId { get; set; }
    public string StripeChargeId { get; set; } = string.Empty; // e.g., "ch_xxxxx"
    public string? StripeTransferId { get; set; } // e.g., "tr_xxxxx" (null until transferred)
    public decimal TotalAmount { get; set; }
    public decimal PlatformFeePercentage { get; set; } // e.g., 5.0 for 5%
    public decimal PlatformFeeAmount { get; set; } // calculated fee amount
    public decimal SellerAmount { get; set; } // amount seller receives (TotalAmount - PlatformFeeAmount)
    public string Currency { get; set; }
    public ListingBasicResponse Listing { get; set; } = null!;
    public UserBasicResponse Buyer { get; set; } = null!;
    public UserBasicResponse Seller { get; set; } = null!;
    public UserBasicResponse ApprovedBy { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public int TransferAttempts { get; set; } = 0;
    public TransferTrigger TransferTrigger { get; set; }
    public TransferMethod? ActualTransferMethod { get; set; }
    public DateTime ChargeDate { get; set; } = DateTime.UtcNow;
    public DateTime? TransferDate { get; set; } // When money was transferred to seller
    public DateTime? ScheduledTransferDate { get; set; } // when it's scheduled for transfer
    public string? Notes { get; set; } // Admin notes, dispute info, etc.
}