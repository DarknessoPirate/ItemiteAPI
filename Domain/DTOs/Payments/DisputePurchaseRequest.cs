using Domain.Enums;

namespace Domain.DTOs.Payments;

public class DisputePurchaseRequest
{
    public DisputeReason Reason { get; set; }
    public string Description { get; set; }
}