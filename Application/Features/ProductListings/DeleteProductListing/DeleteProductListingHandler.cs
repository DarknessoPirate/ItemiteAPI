using Application.Exceptions;
using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.ProductListings.DeleteProductListing;

public class DeleteProductListingHandler(
    ICurrentUserService currentUser,
    IListingRepository<ProductListing> productListingRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<DeleteProductListingHandler> logger
    ) : IRequestHandler<DeleteProductListingCommand>
{
    public async Task Handle(DeleteProductListingCommand request, CancellationToken cancellationToken)
    {
        var listingToDelete = await productListingRepository.GetListingByIdAsync(request.ListingId);
        if (listingToDelete.OwnerId != currentUser.GetId())
        {
            throw new ForbiddenException("You are not allowed to delete this listing");
        }

        try
        {
            productListingRepository.DeleteListing(listingToDelete);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error when deleting product listing {listingToDelete.Id}: {ex.Message}");
            throw;
        }

        await cacheService.RemoveByPatternAsync($"{CacheKeys.PRODUCT_LISTINGS}*");
    }
}