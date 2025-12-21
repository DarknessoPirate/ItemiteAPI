using Application.Features.Payments.ConfirmDelivery;
using Application.Features.Payments.DisputePurchase;
using Application.Features.Payments.GetLatestPayments;
using Application.Features.Payments.GetPaymentCountsByStatus;
using Application.Features.Payments.GetPaymentsByStatus;
using Application.Features.Payments.GetStripeOnboardingStatus;
using Application.Features.Payments.GetUserPurchases;
using Application.Features.Payments.GetUserSales;
using Application.Features.Payments.PurchaseProduct;
using Application.Features.Payments.RefreshStripeOnboarding;
using Application.Features.Payments.ResolveDispute;
using Application.Features.Payments.StartStripeOnboarding;
using Domain.DTOs.File;
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
        var command = new StartStripeOnboardingCommand
        {
            UserId = requestContextService.GetUserId()
        };

        var onboardingUrl = await mediator.Send(command);

        return Ok(new { url = onboardingUrl });
    }

    [HttpGet("stripe/connect/refresh-onboarding-link")]
    public async Task<IActionResult> RefreshOnboardingLink()
    {
        var command = new RefreshStripeOnboardingCommand
        {
            UserId = requestContextService.GetUserId()
        };

        var onboardingUrl = await mediator.Send(command);

        return Redirect(onboardingUrl);
    }

    [Authorize]
    [HttpGet("stripe/onboarding-status")]
    public async Task<IActionResult> GetStripeOnboardingStatus()
    {
        var query = new GetStripeOnboardingStatusQuery
        {
            UserId = requestContextService.GetUserId()
        };

        var isOnboarded = await mediator.Send(query);

        return Ok(new { isOnboarded });
    }

    [Authorize]
    [HttpPost("purchase-product/{productListingId}")]
    public async Task<IActionResult> PurchaseProduct(
        [FromRoute] int productListingId,
        [FromBody] PurchaseProductRequest request)
    {
        var command = new PurchaseProductCommand
        {
            ProductListingId = productListingId,
            PaymentMethodId = request.PaymentMethodId,
            BuyerId = requestContextService.GetUserId()
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }

    [Authorize]
    [HttpGet("my-purchases")]
    public async Task<ActionResult<PageResponse<PaymentBuyerResponse>>> GetMyPurchases([FromQuery] int pageSize,
        [FromQuery] int pageNumber)
    {
        var command = new GetUserPurchasesQuery
        {
            UserId = requestContextService.GetUserId(),
            PageSize = pageSize,
            PageNumber = pageNumber
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }


    [Authorize]
    [HttpGet("my-sales")]
    public async Task<ActionResult<PageResponse<PaymentSellerResponse>>> GetMySales([FromQuery] int pageSize,
        [FromQuery] int pageNumber)
    {
        var command = new GetUserSalesQuery
        {
            UserId = requestContextService.GetUserId(),
            PageSize = pageSize,
            PageNumber = pageNumber
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }

    [Authorize]
    [HttpPost("confirm-delivery/{listindId}")]
    public async Task<IActionResult> ConfirmDelivery([FromRoute] int listindId)
    {
        var command = new ConfirmDeliveryCommand()
        {
            UserId = requestContextService.GetUserId(),
            ListingId = listindId
        };

        await mediator.Send(command);

        return Ok();
    }


    [Authorize]
    [HttpPost("dispute/{paymentId}")]
    public async Task<ActionResult<DisputeResponse>> DisputePurchase([FromRoute] int paymentId,
        [FromForm] DisputePurchaseRequest request, [FromForm] IFormFileCollection photos)
    {
        var command = new DisputePurchaseCommand
        {
            UserId = requestContextService.GetUserId(),
            PaymentId = paymentId,
            Reason = request.Reason,
            Description = request.Description,
            EvidencePhotos = photos.Select(p => new FileWrapper(
                p.FileName,
                p.Length,
                p.ContentType,
                p.OpenReadStream()
            )).ToList()
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }
}