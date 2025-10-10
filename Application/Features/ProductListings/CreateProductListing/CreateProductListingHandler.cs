using AutoMapper;
using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.ProductListings.CreateProductListing;

public class CreateProductListingHandler(
        ICurrentUserService currentUser,
        IListingRepository<ProductListing> productListingRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<CreateProductListingHandler> logger
        ) : IRequestHandler<CreateProductListingCommand, int>
{
    public async Task<int> Handle(CreateProductListingCommand request, CancellationToken cancellationToken)
    {
        var productListing = mapper.Map<ProductListing>(request.ProductListingDto);
        productListing.OwnerId = currentUser.GetId();
        
        var category = await categoryRepository.GetByIdAsync(request.ProductListingDto.CategoryId);
        if (category == null)
        {
            throw new NotFoundException("Category not found");
        }

        if (await categoryRepository.IsParentCategory(request.ProductListingDto.CategoryId))
        {
            throw new BadRequestException("You can't assign parent category to product listing");
        }

        var directParents = await categoryRepository.GetAllParentsRelatedToCategory(category);
        directParents.Add(category);
        
        productListing.Categories = directParents;
        
        // TODO: handle images upload (CloudinaryService needed)

        try
        {
            await productListingRepository.CreateListingAsync(productListing);
            await unitOfWork.SaveChangesAsync();

            await cacheService.RemoveByPatternAsync($"{CacheKeys.PRODUCT_LISTINGS}*");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error when creating new product listing: {ex.Message}");
            throw;
        }
        
        
        return productListing.Id;
    }
}