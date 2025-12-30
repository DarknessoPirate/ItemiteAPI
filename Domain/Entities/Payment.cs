using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    // === Stripe Information ===
    [MaxLength(255)] public string? StripeChargeId { get; set; } = string.Empty; // e.g., "ch_xxxxx"

    [MaxLength(255)] public string? StripeTransferId { get; set; } // e.g., "tr_xxxxx" (null until transferred)

    [Required] public decimal TotalAmount { get; set; }

    [Required] public decimal PlatformFeePercentage { get; set; } // e.g., 5.0 for 5%

    [Required] public decimal PlatformFeeAmount { get; set; } // calculated fee amount

    [Required] public decimal SellerAmount { get; set; } // amount seller receives (TotalAmount - PlatformFeeAmount)

    [Required] [MaxLength(3)] public string Currency { get; set; }

    [Required] public int ListingId { get; set; }
    public ListingBase Listing { get; set; } = null!;

    [Required] public int BuyerId { get; set; }
    public User Buyer { get; set; } = null!;

    [Required] public int SellerId { get; set; }
    public User Seller { get; set; } = null!;

    public Dispute? Dispute { get; set; }

    [Required] public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public int TransferAttempts { get; set; } = 0;

    [Required] public TransferTrigger TransferTrigger { get; set; }

    public TransferMethod? ActualTransferMethod { get; set; }
    public int? ApprovedByUserId { get; set; }
    public User? ApprovedBy { get; set; }

    //
    public DateTime ChargeDate { get; set; } = DateTime.UtcNow;
    public DateTime? TransferDate { get; set; } // When money was transferred to seller
    public DateTime? ScheduledTransferDate { get; set; } // when it's scheduled for transfer
    
    // bid payment data
    public string? StripePaymentIntentId { get; set; }  // The PaymentIntent ID
    public string? PaymentIntentClientSecret { get; set; }  // Frontend needs this for 3D Secure
    public PaymentIntentStatus? PaymentIntentStatus { get; set; } = Enums.PaymentIntentStatus.RequiresAction; // Stripe-specific status
    
    // refund optional data
    public string? StripeRefundId { get; set; } 
    public decimal? RefundAmount { get; set; } 
    public DateTime? RefundDate { get; set; } 
    public DateTime? ScheduledRefundDate { get; set; }
    public int RefundAttempts { get; set; } = 0;
    
    public string? Notes { get; set; } // Admin notes, dispute info, etc.
}