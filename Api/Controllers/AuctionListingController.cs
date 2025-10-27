using Application.Features.Listings.AuctionListings.CreateAuctionListing;
using Application.Features.Listings.AuctionListings.GetAuctionListing;
using Application.Features.Listings.AuctionListings.GetBidHistory;
using Application.Features.Listings.AuctionListings.PlaceBid;
using Application.Features.Listings.AuctionListings.UpdateAuctionListing;
using Domain.DTOs.AuctionListing;
using Domain.DTOs.File;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionListingController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    [HttpGet("{listingId}")]
    public async Task<IActionResult> GetAuctionListing(int listingId)
    {
        var auctionListingQuery = new GetAuctionListingQuery
        {
            ListingId = listingId,
            UserId = requestContextService.GetUserIdNullable()
        };
        var listing = await mediator.Send(auctionListingQuery);
        return Ok(listing);
    }
    
    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateAuctionListing([FromForm] CreateAuctionListingRequest request, [FromForm] List<IFormFile> images)
    {
        var fileWrappers = new List<FileWrapper>();
        foreach (var image in images)
        {
            fileWrappers.Add(new FileWrapper(image.FileName, image.Length, image.ContentType, image.OpenReadStream()));
        }
        var command = new CreateAuctionListingCommand
        {
            AuctionListingDto = request,
            Images = fileWrappers,
            UserId = requestContextService.GetUserId()
        };
        var createdAuctionListingId = await mediator.Send(command);
        return Created($"api/auctionlisting/{createdAuctionListingId}", new {createdAuctionListingId} );
    }

    [Authorize]
    [HttpPost("{listingId}/bid")]
    public async Task<IActionResult> PlaceBid([FromRoute] int listingId, [FromBody] PlaceBidRequest request)
    {
        var command = new PlaceBidCommand
        {
            BidDto = request,
            UserId = requestContextService.GetUserId(),
            AuctionId = listingId
        };
        var createdBidId = await mediator.Send(command);
        return Ok(new {createdBidId});
    }
    
    [HttpGet("{listingId}/bid")]
    public async Task<IActionResult> GetBidsHistory([FromRoute] int listingId)
    {
        var query = new GetBidHistoryQuery
        {
            AuctionId = listingId
        };
        var bids = await mediator.Send(query);
        return Ok(bids);
    }
    
    [Authorize]
    [Consumes("multipart/form-data")]
    [HttpPut("{listingId}")]
    public async Task<IActionResult> UpdateProductListing(
        [FromForm] UpdateAuctionListingRequest request, 
        [FromForm] List<IFormFile> newImages, 
        [FromForm] List<int> newImageOrders,
        [FromRoute] int listingId)
    {
        var fileWrappersWithOrders = new List<FileWrapperWithOrder>();
        for (int i = 0; i < newImages.Count; i++)
        {
            var image = newImages[i];
            var order = i < newImageOrders.Count ? newImageOrders[i] : 2;
            fileWrappersWithOrders.Add(new FileWrapperWithOrder
            {
                File = new FileWrapper(image.FileName, image.Length, image.ContentType, image.OpenReadStream()),
                Order = order
            });
        }
        var command = new UpdateAuctionListingCommand
        {
            UpdateDto = request,
            ListingId = listingId,
            NewImages = fileWrappersWithOrders,
            UserId = requestContextService.GetUserId()
        };
        var updatedProductListing = await mediator.Send(command);
        return Ok(updatedProductListing);
    }
}