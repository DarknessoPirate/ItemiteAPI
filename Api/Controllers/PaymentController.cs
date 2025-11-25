using Application.Features.Payments.PurchaseProduct;
using Application.Features.Payments.RefreshStripeOnboarding;
using Application.Features.Payments.StartStripeOnboarding;
using Domain.DTOs.Payments;
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
// todo: add wrapper for easier success/failure tracking
    [Authorize]
    [HttpPost("purchase-product/{productListingId}")]
    public async Task<IActionResult> PurchaseProduct(
        [FromRoute] int productListingId,
        [FromBody] PurchaseProductRequest request)
    {
        var userId = requestContextService.GetUserId();

        var command = new PurchaseProductCommand
        {
            ProductListingId = productListingId, 
            PaymentMethodId = request.PaymentMethodId, 
            BuyerId = userId
        };

        var paymentId = await mediator.Send(command);

        return Ok(new
        {
            success = true,
            paymentId = paymentId,
            message = "Purchase successful! Payment will be transferred to seller in 7 days."
        });
    }
   
}