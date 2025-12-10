using MediatR;

namespace Application.Features.Payments.ConfirmDelivery;

public class ConfirmDeliveryCommand : IRequest
{
    public int UserId { get; set; }
    public int ListingId { get; set; }
}