using Domain.DTOs.File;
using Domain.DTOs.Payments;
using Domain.Enums;
using MediatR;

namespace Application.Features.Payments.DisputePurchase;

public class DisputePurchaseCommand : IRequest<DisputeResponse>
{
    public int UserId { get; set; }
    public int PaymentId { get; set; }
    public DisputeReason Reason { get; set; }
    public string Description { get; set; }
    public List<FileWrapper>? EvidencePhotos { get; set; }
}