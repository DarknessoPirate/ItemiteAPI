using Domain.Enums;

namespace Domain.DTOs.Payments;

public class ResolveDisputeRequest
{
    public DisputeResolution Resolution { get; set; }
    public decimal? PartialRefundAmount { get; set; }
    public string? ReviewerNotes { get; set; }
}