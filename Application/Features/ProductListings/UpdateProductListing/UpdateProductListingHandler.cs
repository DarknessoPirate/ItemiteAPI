using Application.Exceptions;
using AutoMapper;
using Domain.Configs;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.ProductListings.UpdateProductListing;

public class UpdateProductListingHandler(
    IListingRepository<ProductListing> productListingRepository,
    ICategoryRepository categoryRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IMapper mapper,
    ILogger<UpdateProductListingHandler> logger
    ) : IRequestHandler<UpdateProductListingCommand, ProductListingBasicResponse>
{
    public async Task<ProductListingBasicResponse> Handle(UpdateProductListingCommand request, CancellationToken cancellationToken)
    {
        var productListingToUpdate = await productListingRepository.GetListingWithCategoriesAndOwnerByIdAsync(request.ListingId);
        if (productListingToUpdate == null)
        {
            throw new NotFoundException("Product listing with id " + request.ListingId + " not found");
        }
        
        if (productListingToUpdate.OwnerId != currentUser.GetId())
        {
            throw new ForbiddenException("You are not allowed to update this product");
        }
        
        productListingToUpdate.Name = request.UpdateDto.Name;
        productListingToUpdate.Description = request.UpdateDto.Description;
        productListingToUpdate.Price = request.UpdateDto.Price;
        productListingToUpdate.Location = request.UpdateDto.Location;
        productListingToUpdate.IsNegotiable = request.UpdateDto.IsNegotiable ?? false;
        
        var category = await categoryRepository.GetByIdAsync(request.UpdateDto.CategoryId);
        if (category == null)
        {
            throw new NotFoundException("Category not found");
        }

        if (await categoryRepository.IsParentCategory(request.UpdateDto.CategoryId))
        {
            throw new BadRequestException("You can't assign parent category to product listing");
        }

        var directParents = await categoryRepository.GetAllParentsRelatedToCategory(category);
        directParents.Add(category);
        
        productListingToUpdate.Categories = directParents;
        
        // TODO: handle images upload (CloudinaryService needed)
        
        try
        {
            productListingRepository.UpdateListing(productListingToUpdate);
            await unitOfWork.SaveChangesAsync();
            await cacheService.RemoveByPatternAsync($"{CacheKeys.PRODUCT_LISTINGS}*");
            await cacheService.RemoveAsync($"{CacheKeys.PRODUCT_LISTING}{request.ListingId}");
            return mapper.Map<ProductListingBasicResponse>(productListingToUpdate);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error when updating {request.ListingId}: {ex.Message}");
            throw;
        }
    }
}