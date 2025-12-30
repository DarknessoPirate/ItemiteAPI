namespace Domain.Enums;

public enum PaymentStatus
{
    // Auction-specific statuses
    Authorized, // Bid placed, payment authorized (auction ongoing)
    PendingCapture, // Auction ended, waiting to capture winning bid
    Outbid,
    
    // normal purchase statuses 
    Pending, // charged, waiting to transfer
    PendingReview, // shipment confirmed 
    Transferred, // money sent to seller
    RefundScheduled,
    PartialRefundScheduled,
    Refunded, // refunded to buyer
    PartiallyRefunded,
    Disputed, // under dispute
    Failed
}