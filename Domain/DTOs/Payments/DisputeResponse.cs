using System.ComponentModel.DataAnnotations;
using Domain.DTOs.Listing;
using Domain.DTOs.Photo;
using Domain.DTOs.User;
using Domain.Entities;
using Domain.Enums;

namespace Domain.DTOs.Payments;

public class DisputeResponse
{
    public ListingBasicResponse Listing { get; set; }
    public UserBasicResponse DisputedBy { get; set; }
    public DisputeReason Reason { get; set; }
    public string Description { get; set; }
    public DisputeStatus Status { get; set; }
    public DisputeResolution? Resolution { get; set; }
    public Decimal? RefundAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Notes { get; set; }

    public List<PhotoResponse> Evidence { get; set; } = [];
}