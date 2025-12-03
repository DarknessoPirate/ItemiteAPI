using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class Dispute
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public Payment Payment { get; set; }
    public int InitiatedByUserId { get; set; }
    public User InitiatedBy { get; set; }
    public DisputeReason Reason { get; set; }

    [Required] 
    [MaxLength(500)] public string Description { get; set; }
    public DisputeStatus Status { get; set; }
    public DisputeResolution? Resolution { get; set; }
    public int? ResolvedById { get; set; }
    public User? ResolvedBy { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    public Decimal? RefundAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public ICollection<DisputeEvidence> Evidence { get; set; } = [];
}