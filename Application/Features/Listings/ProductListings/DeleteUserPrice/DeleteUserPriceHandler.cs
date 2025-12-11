using Application.Exceptions;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Listings.ProductListings.DeleteUserPrice;

public class DeleteUserPriceHandler(
    IListingRepository<ProductListing> productListingRepository,
    IUnitOfWork unitOfWork,
    INotificationService notificationService
    ) : IRequestHandler<DeleteUserPriceCommand>
{
    public async Task Handle(DeleteUserPriceCommand request, CancellationToken cancellationToken)
    {
        var productListing = await productListingRepository.GetListingWithPhotosByIdAsync(request.ListingId);
        if (productListing == null)
        {
            throw new NotFoundException("Listing not found");
        }

        if (productListing.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("You are not the owner of this listing");
        }
        
        var userSpecificPrice =
            await productListingRepository.GetUserListingPriceAsync(productListing.Id, request.UserId);

        if (userSpecificPrice == null)
        {
            throw new NotFoundException("User does not have a specific listing price");
        }
        
        productListingRepository.DeleteUserListingPrice(userSpecificPrice);
        
        await unitOfWork.SaveChangesAsync();

        await notificationService.SendNotification([request.UserId], request.OwnerId, new NotificationInfo
        {
            Message = $"Your specific price of the product listing {productListing.Name} has been deleted.",
            ResourceType = ResourceType.Product,
            ResourceId = productListing.Id,
            NotificationImageUrl = productListing.ListingPhotos.First(lp => lp.Order == 1).Photo.Url
        });
    }
}