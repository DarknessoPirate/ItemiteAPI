using Application.Features.Listings.AuctionListings.CreateAuctionListing;
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
}