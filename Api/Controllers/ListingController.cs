using Application.Features.Listings.Shared.DeleteListing;
using Application.Features.Listings.Shared.GetPaginatedListings;
using Application.Features.Listings.Shared.HighlightListing;
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
    public async Task<IActionResult> GetListings([FromQuery] GetPaginatedListingsQuery query)
    {
        var listings = await mediator.Send(query);
        return Ok(listings);
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
}