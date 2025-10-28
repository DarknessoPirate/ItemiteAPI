using Domain.Entities;
using FluentValidation;
using Infrastructure.Interfaces.Repositories;

namespace Application.Features.Listings.AuctionListings.GetBidHistory;

public class GetBidHistoryValidator : AbstractValidator<GetBidHistoryQuery>
{
    private readonly IListingRepository<AuctionListing> _auctionListingRepository;
    public GetBidHistoryValidator(IListingRepository<AuctionListing> auctionListingRepository)
    {
        _auctionListingRepository = auctionListingRepository;
        
        RuleFor(x => x.AuctionId).NotNull().WithMessage("Auction id is required.");
        RuleFor(x => x.AuctionId).MustAsync(AuctionListingExists).WithMessage("Auction not found");
    }

    private async Task<bool> AuctionListingExists(int auctionId, CancellationToken cancellationToken)
    {
        return await _auctionListingRepository.ListingExistsAsync(auctionId);
    }
}