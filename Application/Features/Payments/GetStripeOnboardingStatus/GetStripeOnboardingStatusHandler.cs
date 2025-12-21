using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Payments.GetStripeOnboardingStatus;

public class GetStripeOnboardingStatusHandler(
    UserManager<User> userManager,
    IStripeConnectService stripeConnectService
) : IRequestHandler<GetStripeOnboardingStatusQuery, bool>
{
    public async Task<bool> Handle(GetStripeOnboardingStatusQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("User not found");

        if (string.IsNullOrEmpty(user.StripeConnectAccountId))
            return false;

        return await stripeConnectService.IsAccountFullyOnboardedAsync(user.StripeConnectAccountId);
    }
}
