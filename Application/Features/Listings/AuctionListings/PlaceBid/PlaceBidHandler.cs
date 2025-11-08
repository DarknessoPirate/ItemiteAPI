using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Listings.AuctionListings.PlaceBid;

public class PlaceBidHandler(
    IBidRepository bidRepository,
    IListingRepository<AuctionListing> auctionListingRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<PlaceBidCommand, int>
{
    public async Task<int> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var auction = await auctionListingRepository.GetListingByIdAsync(request.AuctionId);
        if (auction == null)
        {
            throw new NotFoundException($"Auction with id: {request.AuctionId} not found");
        }

        if (auction.OwnerId == request.UserId)
        {
            throw new BadRequestException("You cannot place a bid on an auction you have created");
        }

        var currentHighestBid = auction.CurrentBid ?? auction.StartingBid;
        
        if (request.BidDto.Price <= currentHighestBid)
        {
            throw new BadRequestException("You cannot place an equal or lower bid than current highest bid");
        }

        var bidToAdd = new AuctionBid
        {
            BidPrice = request.BidDto.Price,
            BidderId = request.UserId,
            AuctionId = auction.Id,
        };
        
        await bidRepository.CreateBid(bidToAdd);
        await unitOfWork.SaveChangesAsync();
        
        auction.HighestBidId = bidToAdd.Id;
        auction.CurrentBid = bidToAdd.BidPrice;
        
        auctionListingRepository.UpdateListing(auction);
        await unitOfWork.SaveChangesAsync();

        await cacheService.RemoveAsync($"{CacheKeys.BIDS}{auction.Id}");
        await cacheService.RemoveAsync($"{CacheKeys.AUCTION_LISTING}{auction.Id}");
        
        return bidToAdd.Id;
    }
}