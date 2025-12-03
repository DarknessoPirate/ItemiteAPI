namespace Domain.Enums;

public enum PaymentStatus
{
    Pending, // charged, waiting to transfer
    PendingReview, // shipment confirmed 
    Transferred, // money sent to seller
    Refunded, // refunded to buyer
    Disputed, // under dispute
    Failed
}