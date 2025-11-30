using Application.Features.Payments.GetAllPayments;
using Application.Features.Payments.GetPaymentsByStatus;
using Application.Features.Payments.PurchaseProduct;
using Application.Features.Payments.RefreshStripeOnboarding;
using Application.Features.Payments.StartStripeOnboarding;
using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using Domain.Enums;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using GetPaymentsByStatusQuery = Application.Features.Payments.GetPaymentsByStatus.GetPaymentsByStatusQuery;

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

        return Ok(new PurchaseProductResponse
        {
            PaymentId = paymentId,
            Message = "Purchase successful! Payment will be transferred to seller in 7 days."
        });
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpGet("admin/with-status")]
    public async Task<PageResponse<PaymentResponse>> GetPaymentsByStatus([FromQuery] int pageSize, [FromQuery] int pageNumber, [FromQuery] PaymentStatus paymentStatus)
    {
        var command = new GetPaymentsByStatusQuery
        {
            UserId = requestContextService.GetUserId(),
            PaymentStatus = paymentStatus,
            PageSize = pageSize,
            PageNumber = pageNumber
        };

        return await mediator.Send(command);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpGet]
    public async Task<PageResponse<PaymentResponse>> GetLatestPayments([FromQuery] int pageSize, [FromQuery] int pageNumber)
    {
        var command = new GetLatestPaymentsQuery
        {
            UserId = requestContextService.GetUserId(),
            PageSize = pageSize,
            PageNumber = pageNumber
        };

        return await mediator.Send(command);
    }
}