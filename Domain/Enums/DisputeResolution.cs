namespace Domain.Enums;

public enum DisputeResolution
{
    RefundBuyer, // full refund to buyer
    Declined, // transfer to seller
    PartialRefund, // partial refund to buyer and rest transferred to seller
}