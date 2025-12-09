using MediatR;

namespace Application.Features.Payments.RefreshStripeOnboarding;

public class RefreshStripeOnboardingCommand : IRequest<string>
{
    public int UserId { get; set; }
}