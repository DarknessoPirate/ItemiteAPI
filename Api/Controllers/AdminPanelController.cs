using Application.Features.Categories.CreateCategory;
using Application.Features.Categories.DeleteCategory;
using Application.Features.Categories.UpdateCategory;
using Application.Features.Listings.Shared.DeleteListing;
using Application.Features.Notifications.SendGlobalNotification;
using Application.Features.Notifications.SendNotification;
using Application.Features.Reports.GetPaginatedReports;
using Application.Features.Users.GetPaginatedUsers;
using Application.Features.Users.LockUser;
using Application.Features.Users.UnlockUser;
using Domain.DTOs.Category;
using Domain.DTOs.File;
using Domain.DTOs.Notifications;
using Domain.DTOs.Reports;
using Domain.DTOs.User;
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
    public sealed class FullCategoryRequest
    {
        public CreateCategoryRequest Dto { get; set; }
        public IFormFile? Image { get; set; }
    }
    
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateCategory([FromForm] FullCategoryRequest request)
    {
        var command = new CreateCategoryCommand
        {
            CreateCategoryDto = request.Dto,
            Image = request.Image != null ? new FileWrapper(request.Image.FileName, request.Image.Length, request.Image.ContentType, request.Image.OpenReadStream()) : null
        };

        int categoryId = await mediator.Send(command);

        return Created($"api/category/{categoryId}", new { categoryId });
    }
    
    [HttpPut("{categoryId:int}")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(int categoryId,[FromBody] UpdateCategoryRequest updateCategoryRequest)
    {
        var command = new UpdateCategoryCommand
        {
            CategoryId = categoryId,
            Dto = updateCategoryRequest
        };

        var result = await mediator.Send(command);
        
        return Ok(result);
    }
    
    
    [HttpDelete("{categoryId:int}")]
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
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request, [FromRoute] int userId)
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
}