namespace Domain.Enums;

public enum PaymentStatus
{
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