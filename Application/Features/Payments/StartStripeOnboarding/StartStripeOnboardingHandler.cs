using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Application.Features.Payments.StartStripeOnboarding;

/// <summary>
/// Starts the stripe onboarding with the user account's email and generates a link
/// to the stripe website to fully finish the onboarding. For the stripe connect
/// service to work user needs to be fully onboarded using this generated link.
/// If stripe onboarding is not complete, user can't receive payments.
/// </summary>
public class StartStripeOnboardingHandler(
    IStripeConnectService stripeConnectService,
    UserManager<User> userManager,
    IOptions<RedirectSettings> redirectSettings
) : IRequestHandler<StartStripeOnboardingCommand, string>
{
    public async Task<string> Handle(StartStripeOnboardingCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("Invalid user id");

        var returnUrl = redirectSettings.Value.StripeReturnOnboardingUrl;
        var refreshUrl = redirectSettings.Value.StripeRefreshOnboardingUrl;

        if (!string.IsNullOrEmpty(user.StripeConnectAccountId) && await stripeConnectService.IsAccountFullyOnboardedAsync(user.StripeConnectAccountId))
            throw new BadRequestException("You already have the stripe account set up");


        var onboardingUrl = await stripeConnectService.CreateConnectAccountAndGetOnboardingUrlAsync(
            user,
            returnUrl,
            refreshUrl);

        return onboardingUrl;
    }
}