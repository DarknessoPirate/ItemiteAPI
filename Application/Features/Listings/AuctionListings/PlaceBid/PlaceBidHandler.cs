using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.AuctionListings.PlaceBid;

public class PlaceBidHandler(
    IBidRepository bidRepository,
    IListingRepository<AuctionListing> auctionListingRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    UserManager<User> userManager,
    ILogger<PlaceBidHandler> logger
    ) : IRequestHandler<PlaceBidCommand, int>
{
    public async Task<int> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var auction = await auctionListingRepository.GetListingByIdAsync(request.AuctionId);
        if (auction == null)
        {
            throw new NotFoundException($"Auction with id: {request.AuctionId} not found");
        }
        
        var bidder = await userManager.FindByIdAsync(request.UserId.ToString());

        if (bidder == null)
        {
            throw new NotFoundException($"User with id: {request.UserId} not found");
        }

        if (auction.OwnerId == request.UserId)
        {
            throw new BadRequestException("You cannot place a bid on an auction you have created");
        }

        if (auction.DateEnds <= DateTime.UtcNow)
        {
            throw new BadRequestException("Auction has ended");
        }
        
        var formerHighestBid = await bidRepository.GetCurrentHighestBid(auction.Id);
        
        var currentHighestBidValue = auction.CurrentBid ?? auction.StartingBid;
        
        if (request.BidDto.Price <= currentHighestBidValue)
        {
            throw new BadRequestException("You cannot place an equal or lower bid than current highest bid");
        }

        var bidToAdd = new AuctionBid
        {
            BidPrice = request.BidDto.Price,
            BidderId = request.UserId,
            AuctionId = auction.Id,
        };
        await unitOfWork.BeginTransactionAsync();
        try
        {
            await bidRepository.CreateBid(bidToAdd);
            await unitOfWork.SaveChangesAsync();

            auction.HighestBidId = bidToAdd.Id;
            auction.CurrentBid = bidToAdd.BidPrice;

            auctionListingRepository.UpdateListing(auction);
            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error when placing a bid: {ex.Message}");
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
      

        await cacheService.RemoveAsync($"{CacheKeys.BIDS}{auction.Id}");
        await cacheService.RemoveAsync($"{CacheKeys.AUCTION_LISTING}{auction.Id}");

        await notificationService.SendNotification([auction.OwnerId], request.UserId, new NotificationInfo
        {
            Message = $"User {bidder.UserName} placed a new bid with value: {bidToAdd.BidPrice} in your auction {auction.Name}",
            ResourceId = request.AuctionId,
            ResourceType = ResourceType.Auction,
            NotificationImageUrl = auction.ListingPhotos.First(p => p.Order == 1).Photo.Url,
        });

        if (formerHighestBid != null)
        {
            await notificationService.SendNotification([formerHighestBid.BidderId], request.UserId, new NotificationInfo
            {
                Message = $"User {bidder.UserName} placed a new bid with value: {bidToAdd.BidPrice} in auction {auction.Name}. You are no longer the highest bidder.",
                ResourceId = request.AuctionId,
                ResourceType = ResourceType.Auction,
                NotificationImageUrl = auction.ListingPhotos.First(p => p.Order == 1).Photo.Url,
            });
        }
        
        return bidToAdd.Id;
    }
}