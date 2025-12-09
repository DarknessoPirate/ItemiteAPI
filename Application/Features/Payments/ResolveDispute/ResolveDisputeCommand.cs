using Domain.DTOs.Payments;
using Domain.Enums;
using MediatR;

namespace Application.Features.Payments.ResolveDispute;

public class ResolveDisputeCommand : IRequest<DisputeResponse>
{
   public int AdminUserId { get; set; }
   public int DisputeId { get; set; }
   public DisputeResolution Resolution { get; set; }
   public decimal? PartialRefundAmount { get; set; }
   public string? ReviewerNotes { get; set; }
}