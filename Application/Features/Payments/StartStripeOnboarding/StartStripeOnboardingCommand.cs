using MediatR;

namespace Application.Features.Payments.StartStripeOnboarding;

public class StartStripeOnboardingCommand : IRequest<string>
{
    public int UserId { get; set; }
}