using MediatR;

namespace Application.Features.Payments.GetStripeOnboardingStatus;

public class GetStripeOnboardingStatusQuery : IRequest<bool>
{
    public int UserId { get; set; }
}
