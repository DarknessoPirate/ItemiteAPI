using Application.Exceptions;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Listings.ProductListings.SetUserPrice;

public class SetUserPriceHandler(
    IListingRepository<ProductListing> productListingRepository,
    IUnitOfWork unitOfWork,
    INotificationService notificationService
    ) : IRequestHandler<SetUserPriceCommand>
{
    public async Task Handle(SetUserPriceCommand request, CancellationToken cancellationToken)
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

        if (productListing.IsSold)
        {
            throw new BadRequestException("Listing has been sold");
        }

        if (productListing.IsArchived)
        {
            throw new BadRequestException("Listing has been archived");
        }

        if (request.Dto.Price == productListing.Price)
        {
            throw new BadRequestException("Price for user can't be the same as current listing price");
        }
        
        
        
        if (request.UserId == request.OwnerId)
        {
            throw new ForbiddenException("You can't change price for yourself");
        }

        var userSpecificPrice =
            await productListingRepository.GetUserListingPriceAsync(productListing.Id, request.UserId);
        
        if (userSpecificPrice == null)
        {
            var userPriceToAdd = new UserListingPrice
            {
                ListingId = productListing.Id,
                UserId = request.UserId,
                Price = request.Dto.Price,
            };
            await productListingRepository.AddUserListingPriceAsync(userPriceToAdd);
        }
        else
        {
            if (request.Dto.Price == userSpecificPrice.Price)
            {
                throw new BadRequestException("The new price cannot be the same as current user price");
            }
            userSpecificPrice.Price = request.Dto.Price;
            productListingRepository.UpdateUserListingPrice(userSpecificPrice);
        }
        await unitOfWork.SaveChangesAsync();

        await notificationService.SendNotification([request.UserId], request.OwnerId, new NotificationInfo
        {
            Message = $"The price of the product listing {productListing.Name} has changed for you from {productListing.Price} to {request.Dto.Price}!",
            ResourceType = ResourceType.Product,
            ResourceId = productListing.Id,
            NotificationImageUrl = productListing.ListingPhotos.First(lp => lp.Order == 1).Photo.Url
        });
    }
}