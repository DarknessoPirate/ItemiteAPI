using Application.Features.ProductListings.CreateProductListing;
using Application.Features.ProductListings.DeleteProductListing;
using Application.Features.ProductListings.GetPaginatedProductListings;
using Application.Features.ProductListings.GetProductListing;
using Application.Features.ProductListings.UpdateProductListing;
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
    [HttpGet]
    public async Task<IActionResult> GetProductListings([FromQuery] GetPaginatedProductListingsQuery query)
    {
        var productListings = await mediator.Send(query);
        return Ok(productListings);
    }

    [HttpGet("{listingId}")]
    public async Task<IActionResult> GetProductListingById(int listingId)
    {
        var productListingQuery = new GetProductListingQuery
        {
            ListingId = listingId,
            UserId = requestContextService.GetUserId()
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
    [HttpDelete("{listingId}")]
    public async Task<IActionResult> DeleteProductListing([FromRoute] int listingId)
    {
        var command = new DeleteProductListingCommand
        {
            ListingId = listingId,
            UserId = requestContextService.GetUserId()
        };
        await mediator.Send(command);
        return NoContent();
    }
    
    [Authorize]
    [HttpPut("{listingId}")]
    public async Task<IActionResult> UpdateProductListing([FromBody] UpdateProductListingRequest request, [FromRoute] int listingId)
    {
        var command = new UpdateProductListingCommand
        {
            UpdateDto = request,
            ListingId = listingId,
            UserId = requestContextService.GetUserId()
        };
        var updatedProductListing = await mediator.Send(command);
        return Ok(updatedProductListing);
    }
}