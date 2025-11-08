using Application.Exceptions;
using Application.Features.Listings.ProductListings.UpdateProductListing;
using AutoMapper;
using Domain.Configs;
using Domain.DTOs.AuctionListing;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.AuctionListings.UpdateAuctionListing;

public class UpdateAuctionListingHandler(
    IListingRepository<AuctionListing> auctionListingRepository,
    ICategoryRepository categoryRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService,
    ICacheService cacheService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateAuctionListingHandler> logger
    ) : IRequestHandler<UpdateAuctionListingCommand, AuctionListingResponse>
{
    public async Task<AuctionListingResponse> Handle(UpdateAuctionListingCommand request, CancellationToken cancellationToken)
    {
        var auctionListingToUpdate = await auctionListingRepository.GetListingByIdAsync(request.ListingId);
        if (auctionListingToUpdate == null)
        {
            throw new NotFoundException("Auction listing with id " + request.ListingId + " not found");
        }
        
        if (auctionListingToUpdate.OwnerId != request.UserId)
        {
            throw new ForbiddenException("You are not allowed to update this auction");
        }
        
        auctionListingToUpdate.Name = request.UpdateDto.Name;
        auctionListingToUpdate.Description = request.UpdateDto.Description;
        
        
        if (request.UpdateDto.StartingBid > auctionListingToUpdate.CurrentBid)
        {
            throw new BadRequestException("Starting bid can't be greater than current bid");
        }
        auctionListingToUpdate.StartingBid = request.UpdateDto.StartingBid;
        
        if (request.UpdateDto.Location == null || !IsLocationComplete(request.UpdateDto.Location))
        {
            var ownerLoc = auctionListingToUpdate.Owner.Location;
            if (ownerLoc == null || !IsLocationComplete(ownerLoc))
            {
                throw new BadRequestException("Location is required. Please provide location or set your profile location.");
            }
   
            auctionListingToUpdate.Location.Longitude = ownerLoc.Longitude;
            auctionListingToUpdate.Location.Latitude = ownerLoc.Latitude;
            auctionListingToUpdate.Location.Country = ownerLoc.Country;
            auctionListingToUpdate.Location.City = ownerLoc.City;
            auctionListingToUpdate.Location.State = ownerLoc.State;
        }
        else
        {
            var loc = request.UpdateDto.Location;
            auctionListingToUpdate.Location.Longitude = loc.Longitude;
            auctionListingToUpdate.Location.Latitude = loc.Latitude;
            auctionListingToUpdate.Location.Country = loc.Country;
            auctionListingToUpdate.Location.City = loc.City;
            auctionListingToUpdate.Location.State = loc.State;
        }
        
        var category = await categoryRepository.GetByIdAsync(request.UpdateDto.CategoryId);
        if (category == null)
        {
            throw new NotFoundException("Category not found");
        }

        if (await categoryRepository.IsParentCategory(request.UpdateDto.CategoryId))
        {
            throw new BadRequestException("You can't assign parent category to auction listing");
        }

        var directParents = await categoryRepository.GetAllParentsRelatedToCategory(category);
        directParents.Add(category);
        
        auctionListingToUpdate.Categories = directParents;

        await unitOfWork.BeginTransactionAsync();
        try
        {
            var allListingPhotos = auctionListingToUpdate.ListingPhotos.ToList();
            if (request.UpdateDto.ExistingPhotoOrders == null && request.UpdateDto.ExistingPhotoIds == null)
            {
                foreach (var listingPhoto in allListingPhotos)
                {
                    var deletionResult = await mediaService.DeleteImageAsync(listingPhoto.Photo.PublicId);
                    if (deletionResult.Error != null)
                    {
                        throw new CloudinaryException($"Failed to delete photo {listingPhoto.Photo.Id}: {deletionResult.Error.Message}");
                    }
                    await photoRepository.DeletePhotoAsync(listingPhoto.PhotoId);
                    auctionListingToUpdate.ListingPhotos.Remove(listingPhoto);
                }
            }
            else if (request.UpdateDto.ExistingPhotoOrders != null && request.UpdateDto.ExistingPhotoIds != null)
            {
                var photosToDelete = allListingPhotos
                    .Where(lp => !request.UpdateDto.ExistingPhotoIds.Contains(lp.PhotoId))
                    .ToList();
                
                foreach (var listingPhoto in photosToDelete)
                {
                    var deletionResult = await mediaService.DeleteImageAsync(listingPhoto.Photo.PublicId);
                    if (deletionResult.Error != null)
                    {
                        throw new CloudinaryException($"Failed to delete photo {listingPhoto.Photo.Id}: {deletionResult.Error.Message}");
                    }
                    await photoRepository.DeletePhotoAsync(listingPhoto.PhotoId);
                    auctionListingToUpdate.ListingPhotos.Remove(listingPhoto);
                }
               
                for (int i = 0; i < request.UpdateDto.ExistingPhotoIds.Count; i++)
                {
                    var photoId = request.UpdateDto.ExistingPhotoIds[i];
                    var newOrder = request.UpdateDto.ExistingPhotoOrders[i];
                    
                    var listingPhoto = auctionListingToUpdate.ListingPhotos
                        .FirstOrDefault(lp => lp.PhotoId == photoId);
        
                    if (listingPhoto != null)
                    {
                        listingPhoto.Order = newOrder;
                    }
                }
            }
            
            if (request.NewImages != null && request.NewImages.Any())
            {
                var savedPhotosPublicIds = new List<string>();
    
                for (int i = 0; i < request.NewImages.Count; i++)
                {
                    var image = request.NewImages[i];
                    var uploadResult = await mediaService.UploadPhotoAsync(image.File);
        
                    if (uploadResult.Error != null)
                    {
                        foreach (var savedPhoto in savedPhotosPublicIds)
                        {
                            await mediaService.DeleteImageAsync(savedPhoto);
                        }
                        throw new CloudinaryException(uploadResult.Error.Message);
                    }
        
                    savedPhotosPublicIds.Add(uploadResult.PublicId);
        
                    var photo = new Photo
                    {
                        Url = uploadResult.SecureUrl.AbsoluteUri,
                        PublicId = uploadResult.PublicId,
                        FileName = image.File.FileName
                    };
        
                    await photoRepository.AddPhotoAsync(photo);
                    
                    int order = request.NewImages != null && request.NewImages.Count > i
                        ? request.NewImages.Select(i => i.Order).ToArray()[i]
                        : auctionListingToUpdate.ListingPhotos.Max(lp => lp.Order) + i + 1;
        
                    var listingPhoto = new ListingPhoto
                    {
                        Photo = photo,
                        Order = order
                    };
        
                    auctionListingToUpdate.ListingPhotos.Add(listingPhoto);
                }
            }

            auctionListingRepository.UpdateListing(auctionListingToUpdate);
            await unitOfWork.CommitTransactionAsync();

            await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}*");
            await cacheService.RemoveAsync($"{CacheKeys.AUCTION_LISTING}{request.ListingId}");

            return mapper.Map<AuctionListingResponse>(auctionListingToUpdate);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error when updating {request.ListingId}: {ex.Message}");
            throw;
        }
    }
    private bool IsLocationComplete(Location? location)
    {
        if (location == null) return false;
    
        return location.Longitude.HasValue 
               && location.Latitude.HasValue 
               && !string.IsNullOrWhiteSpace(location.Country) 
               && !string.IsNullOrWhiteSpace(location.City) 
               && !string.IsNullOrWhiteSpace(location.State);
    }
}