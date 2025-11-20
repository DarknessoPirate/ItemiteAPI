using Application.Features.Listings.Shared.DeleteListing;
using Application.Features.Listings.Shared.FollowListing;
using Application.Features.Listings.Shared.GetPaginatedFollowedListings;
using Application.Features.Listings.Shared.GetPaginatedListings;
using Application.Features.Listings.Shared.GetPaginatedUserListings;
using Application.Features.Listings.Shared.HighlightListing;
using Application.Features.Listings.Shared.UnfollowListing;
using Domain.DTOs.Listing;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetListings([FromQuery] PaginateListingQuery query)
    {
        var getListingsQuery = new GetPaginatedListingsQuery
        {
            Query = query,
            UserId = requestContextService.GetUserIdNullable()
        };
        var listings = await mediator.Send(getListingsQuery);
        return Ok(listings);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserListings([FromRoute] int userId, [FromQuery] PaginateUserListingsQuery query)
    {
        var getUserListingsQuery = new GetPaginatedUserListingsQuery
        {
            Query = query,
            UserId = userId,
            CurrentUserId = requestContextService.GetUserIdNullable()
        };
        var listings = await mediator.Send(getUserListingsQuery);
        return Ok(listings);
    }

    [Authorize]
    [HttpGet("follow")]
    public async Task<IActionResult> GetFollowedListings([FromQuery] PaginateFollowedListingsQuery query)
    {
        var getFollowedListingsQuery = new GetPaginatedFollowedListingsQuery
        {
            Query = query,
            UserId = requestContextService.GetUserId()
        };
        
        var followedListings = await mediator.Send(getFollowedListingsQuery);
        return Ok(followedListings); 
    }
    
    [Authorize]
    [HttpDelete("{listingId}")]
    public async Task<IActionResult> DeleteProductListing([FromRoute] int listingId)
    {
        var command = new DeleteListingCommand
        {
            ListingId = listingId,
            UserId = requestContextService.GetUserId()
        };
        await mediator.Send(command);
        return NoContent();
    }

    [Authorize]
    [HttpPost("feature")]
    public async Task<IActionResult> FeatureListing([FromBody] List<int> listingIdsToFeature)
    {
        var command = new HighlightListingCommand
        {
            ListingIds = listingIdsToFeature,
            UserId = requestContextService.GetUserId()
        };
        
        var resultMessage = await mediator.Send(command);
        return Ok(new { resultMessage });
    }

    [Authorize]
    [HttpPost("follow/{listingId}")]
    public async Task<IActionResult> FollowListing([FromRoute] int listingId)
    {
        var command = new FollowListingCommand
        {
            ListingId = listingId,
            UserId = requestContextService.GetUserId()
        };
        
        var createdFollowId = await mediator.Send(command);
        
        return Created("api/listing/followed", new {createdFollowId});
    }

    [Authorize]
    [HttpDelete("follow/{listingId}")]
    public async Task<IActionResult> UnfollowListing([FromRoute] int listingId)
    {
        var command = new UnfollowListingCommand
        {
            ListingId = listingId,
            UserId = requestContextService.GetUserId()
        };
        
        await mediator.Send(command);
        return NoContent();
    }
}