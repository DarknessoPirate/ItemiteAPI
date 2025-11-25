using Application.Features.Payments.RefreshStripeOnboarding;
using Application.Features.Payments.StartStripeOnboarding;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    [Authorize]
    [HttpPost("stripe/connect/start")]
    public async Task<IActionResult> StartStripeConnect()
    {
        var userId = requestContextService.GetUserId();
        var command = new StartStripeOnboardingCommand
        {
            UserId = userId,
        };

        var onboardingUrl = await mediator.Send(command);

        return Ok(new { url = onboardingUrl });
    }

    [HttpGet("stripe/connect/refresh-onboarding-link")]
    public async Task<IActionResult> RefreshOnboardingLink()
    {
        var userId = requestContextService.GetUserId();

        var command = new RefreshStripeOnboardingCommand
        {
            UserId = userId
        };

        var onboardingUrl = await mediator.Send(command);

        return Redirect(onboardingUrl);
    }
}