using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Application.Features.Payments.StartStripeOnboarding;

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