using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Application.Features.Payments.RefreshStripeOnboarding;

/// <summary>
/// If the original link does not work, the user is routed to the endpoint that
/// uses this handler to generate a fresh onboarding link for the user's account
/// </summary>
public class RefreshStripeOnboardingHandler(
    UserManager<User> userManager,
    IStripeConnectService stripeConnectService,
    IOptions<RedirectSettings> redirectSettings
) : IRequestHandler<RefreshStripeOnboardingCommand, string>
{
    public async Task<string> Handle(RefreshStripeOnboardingCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("User not found");

        if (string.IsNullOrEmpty(user.StripeConnectAccountId))
            throw new BadRequestException("User doesn't have a Stripe Connect account");

        var returnUrl = redirectSettings.Value.StripeReturnOnboardingUrl;
        var refreshUrl = redirectSettings.Value.StripeRefreshOnboardingUrl;

        return await stripeConnectService.GenerateOnboardingLinkAsync(
            user.StripeConnectAccountId,
            returnUrl,
            refreshUrl
        );
    }
}