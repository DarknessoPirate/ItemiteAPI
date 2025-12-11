using Application.Features.Listings.ProductListings.CreateProductListing;
using Application.Features.Listings.ProductListings.DeleteUserPrice;
using Application.Features.Listings.ProductListings.GetProductListing;
using Application.Features.Listings.ProductListings.SetUserPrice;
using Application.Features.Listings.ProductListings.UpdateProductListing;
using Domain.DTOs.File;
using Domain.DTOs.ProductListing;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductListingController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    
    [HttpGet("{listingId}")]
    public async Task<IActionResult> GetProductListingById(int listingId)
    {
        var productListingQuery = new GetProductListingQuery
        {
            ListingId = listingId,
            UserId = requestContextService.GetUserIdNullable()
        };
        var listing = await mediator.Send(productListingQuery);
        return Ok(listing);
    }
    
    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateProductListing([FromForm] CreateProductListingRequest request, [FromForm] List<IFormFile> images)
    {
        var fileWrappers = new List<FileWrapper>();
        foreach (var image in images)
        {
            fileWrappers.Add(new FileWrapper(image.FileName, image.Length, image.ContentType, image.OpenReadStream()));
        }
        var command = new CreateProductListingCommand
        {
            ProductListingDto = request,
            Images = fileWrappers,
            UserId = requestContextService.GetUserId()
        };
        var createdProductListingId = await mediator.Send(command);
        return Created($"api/productlisting/{createdProductListingId}", new {createdProductListingId} );
    }
    
    [Authorize]
    [Consumes("multipart/form-data")]
    [HttpPut("{listingId}")]
    public async Task<IActionResult> UpdateProductListing(
        [FromForm] UpdateProductListingRequest request, 
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
        var command = new UpdateProductListingCommand
        {
            UpdateDto = request,
            ListingId = listingId,
            NewImages = fileWrappersWithOrders,
            UserId = requestContextService.GetUserId()
        };
        var updatedProductListing = await mediator.Send(command);
        return Ok(updatedProductListing);
    }

    [Authorize]
    [HttpPost("{listingId}/user-price/{userId}")]
    public async Task<IActionResult> SetUserPrice([FromBody] SetUserPriceRequest request, [FromRoute] int listingId, [FromRoute] int userId)
    {
        var command = new SetUserPriceCommand
        {
            OwnerId = requestContextService.GetUserId(),
            UserId = userId,
            ListingId = listingId,
            Dto = request,
        };
        
        await mediator.Send(command);
        return Created();
    }

    [Authorize]
    [HttpDelete("{listingId}/user-price/{userId}")]
    public async Task<IActionResult> DeleteUserPrice([FromRoute] int listingId, [FromRoute] int userId)
    {
        var command = new DeleteUserPriceCommand
        {
            OwnerId = requestContextService.GetUserId(),
            UserId = userId,
            ListingId = listingId
        };
        await mediator.Send(command);
        return NoContent();
    }
    
}