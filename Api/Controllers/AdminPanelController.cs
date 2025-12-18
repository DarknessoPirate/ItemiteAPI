using Application.Features.Banners.AddBanner;
using Application.Features.Banners.DeleteBanner;
using Application.Features.Banners.GetAllBanners;
using Application.Features.Listings.Shared.DeleteListing;
using Application.Features.Notifications.SendGlobalNotification;
using Application.Features.Notifications.SendNotification;
using Application.Features.Payments.GetLatestPayments;
using Application.Features.Payments.GetPaymentCountsByStatus;
using Application.Features.Payments.GetPaymentsByStatus;
using Application.Features.Payments.ResolveDispute;
using Application.Features.Reports.GetPaginatedReports;
using Application.Features.Users.GetPaginatedUsers;
using Application.Features.Users.LockUser;
using Application.Features.Users.UnlockUser;
using Domain.DTOs.Banners;
using Domain.DTOs.File;
using Domain.DTOs.Notifications;
using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using Domain.DTOs.Reports;
using Domain.DTOs.User;
using Domain.Enums;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Moderator")]
public class AdminPanelController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    [HttpPost("global-notification")]
    public async Task<IActionResult> SendGlobalNotification(SendNotificationRequest request)
    {
        var command = new SendGlobalNotificationCommand
        {
            Dto = request,
            UserId = requestContextService.GetUserId()
        };

        await mediator.Send(command);
        return Ok();
    }

    [HttpPost("notification/{userId}")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request,
        [FromRoute] int userId)
    {
        var command = new SendNotificationCommand
        {
            RecipientId = userId,
            SendNotificationDto = request,
            UserId = requestContextService.GetUserId()
        };

        await mediator.Send(command);
        return Ok();
    }

    [HttpDelete("{listingId}")]
    public async Task<IActionResult> DeleteListing([FromRoute] int listingId)
    {
        var command = new DeleteListingCommand
        {
            ListingId = listingId,
            UserId = requestContextService.GetUserId()
        };
        await mediator.Send(command);
        return NoContent();
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports([FromQuery] PaginateReportsQuery query)
    {
        var reportsQuery = new GetPaginatedReportsQuery
        {
            Query = query
        };
        var reports = await mediator.Send(reportsQuery);
        return Ok(reports);
    }

    [HttpPost("lock-user")]
    public async Task<IActionResult> LockUser([FromBody] LockUserRequest dto)
    {
        var command = new LockUserCommand
        {
            LockUserDto = dto
        };
        await mediator.Send(command);
        return Ok();
    }

    [HttpPost("unlock-user")]
    public async Task<IActionResult> UnlockUser([FromBody] UnlockUserRequest dto)
    {
        var command = new UnlockUserCommand
        {
            UnlockUserDto = dto
        };
        await mediator.Send(command);
        return Ok();
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] PaginateUsersQuery query)
    {
        var getUsersQuery = new GetPaginatedUsersQuery
        {
            Query = query
        };
        var users = await mediator.Send(getUsersQuery);
        return Ok(users);
    }


    [HttpGet("payments/with-status")]
    public async Task<ActionResult<PageResponse<PaymentResponse>>> GetPaymentsByStatus([FromQuery] int pageSize,
        [FromQuery] int pageNumber, [FromQuery] PaymentStatus paymentStatus)
    {
        var command = new GetPaymentsByStatusQuery
        {
            AdminUserId = requestContextService.GetUserId(),
            PaymentStatus = paymentStatus,
            PageSize = pageSize,
            PageNumber = pageNumber
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }


    [HttpGet("payments/latest")]
    public async Task<ActionResult<PageResponse<PaymentResponse>>> GetLatestPayments([FromQuery] int pageSize,
        [FromQuery] int pageNumber)
    {
        var command = new GetLatestPaymentsQuery
        {
            AdminUserId = requestContextService.GetUserId(),
            PageSize = pageSize,
            PageNumber = pageNumber
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }


    [HttpGet("payments/counts")]
    public async Task<ActionResult<PaymentStatusCountsResponse>> GetPaymentCountsByStatus()
    {
        var command = new GetPaymentCountsByStatusQuery
        {
            AdminUserId = requestContextService.GetUserId()
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }


    [HttpPost("dispute/resolve/{disputeId}")]
    public async Task<ActionResult<DisputeResponse>> ResolveDispute([FromRoute] int disputeId,
        [FromBody] ResolveDisputeRequest request)
    {
        var command = new ResolveDisputeCommand
        {
            AdminUserId = requestContextService.GetUserId(),
            DisputeId = disputeId,
            Resolution = request.Resolution,
            PartialRefundAmount = request.PartialRefundAmount,
            ReviewerNotes = request.ReviewerNotes
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }


    [HttpPost("banners")]
    public async Task<ActionResult<BannerResponse>> AddBanner([FromForm] AddBannerRequest request, IFormFile photo)
    {
        var command = new AddBannerCommand
        {
            Dto = request,
            UserId = requestContextService.GetUserId(),
            BannerPhoto = new FileWrapper(photo.FileName,
                photo.Length,
                photo.ContentType,
                photo.OpenReadStream())
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }


    [HttpDelete("banners/{bannerId}")]
    public async Task<IActionResult> DeleteBanner([FromRoute] int bannerId)
    {
        var command = new DeleteBannerCommand
        {
            UserId = requestContextService.GetUserId(),
            BannerId = bannerId
        };

        await mediator.Send(command);

        return NoContent();
    }


    [HttpGet("banners/all")]
    public async Task<ActionResult<List<BannerResponse>>> GetAllBanners()
    {
        var command = new GetAllBannersQuery
        {
            UserId = requestContextService.GetUserId()
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }

    /*

    [HttpPut("banners/{bannerId}")]
    public async Task<ActionResult<>> UpdateBanner([FromRoute] int bannerId        )

    {

    }







    [HttpPost("banners/active/{bannerId}")]
    public async Task<ActionResult<>> ToggleActiveBanner([FromRoute] int bannerId         )
    {

    }



    [HttpGet("banners/active")]
    public async Task<ActionResult<>> GetActiveBanners(                )
    {

    }

    */
}