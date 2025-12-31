using Application.Features.Categories.CreateCategory;
using Application.Features.Categories.DeleteCategory;
using Application.Features.Categories.UpdateCategory;
using Application.Features.Banners.AddBanner;
using Application.Features.Banners.DeleteBanner;
using Application.Features.Banners.GetAllBanners;
using Application.Features.Banners.ToggleActiveBanner;
using Application.Features.Banners.UpdateBanner;
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
using Domain.DTOs.Category;
using Domain.DTOs.File;
using Domain.DTOs.Banners;
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
    
    [HttpPost("category")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateCategory([FromForm] CreateCategoryRequest request, IFormFile? image)
    {
        var command = new CreateCategoryCommand
        {
            CreateCategoryDto = request,
            Image = image != null ? new FileWrapper(image.FileName, image.Length, image.ContentType, image.OpenReadStream()) : null
        };

        int categoryId = await mediator.Send(command);

        return Created($"api/category/{categoryId}", new { categoryId });
    }
    
    [HttpPut("category/{categoryId:int}")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(int categoryId,[FromForm] UpdateCategoryRequest updateCategoryRequest, IFormFile? image)
    {
        var command = new UpdateCategoryCommand
        {
            CategoryId = categoryId,
            Dto = updateCategoryRequest,
            Image = image != null ? new FileWrapper(image.FileName, image.Length, image.ContentType, image.OpenReadStream()) : null
        };

        var result = await mediator.Send(command);
        
        return Ok(result);
    }
    
    
    [HttpDelete("category/{categoryId:int}")]
    public async Task<IActionResult> DeleteCategory(int categoryId, [FromQuery] bool deleteFullTree = false)
    {
        var command = new DeleteCategoryCommand
        {
            CategoryId = categoryId,
            DeleteFullTree = deleteFullTree
        };
        
        await mediator.Send(command);

        return NoContent();
    }
    
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


    [HttpPut("banners/{bannerId}")]
    public async Task<ActionResult<BannerResponse>> UpdateBanner([FromRoute] int bannerId,
        [FromForm] UpdateBannerRequest request, IFormFile? photo)
    {
        var command = new UpdateBannerCommand
        {
            BannerId = bannerId,
            Dto = request,
            UserId = requestContextService.GetUserId(),
            BannerPhoto = photo != null
                ? new FileWrapper(photo.FileName, photo.Length, photo.ContentType, photo.OpenReadStream())
                : null
        };

        var response = await mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("banners/active/{bannerId}")]
    public async Task<ActionResult<BannerResponse>> ToggleActiveBanner([FromRoute] int bannerId)
    {
        var command = new ToggleActiveBannerCommand
        {
            BannerId = bannerId,
            UserId = requestContextService.GetUserId()
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
}