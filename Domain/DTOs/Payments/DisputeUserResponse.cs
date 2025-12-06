using Domain.DTOs.Photo;
using Domain.DTOs.User;
using Domain.Enums;

namespace Domain.DTOs.Payments;

public class DisputeUserResponse
{
    public int Id { get; set; }
    public UserBasicResponse DisputedBy { get; set; }
    public DisputeReason Reason { get; set; }
    public string Description { get; set; }
    public DisputeStatus Status { get; set; }
    public DisputeResolution? Resolution { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolvedByAdmin { get; set; }
    public List<PhotoResponse> Evidence { get; set; } = [];
    public string? Notes { get; set; }
}