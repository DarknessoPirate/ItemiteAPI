using AutoMapper;
using Domain.DTOs.Listing;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.Shared.GetUserDedicatedListings;

public class GetUserDedicatedListingsHandler(
    IListingRepository<ListingBase> listingRepository,
    ILIstingViewRepository listingViewRepository,
    IMapper mapper,
    ILogger<GetUserDedicatedListingsHandler> logger
    ) : IRequestHandler<GetUserDedicatedListingsQuery, List<ListingBasicResponse>>
{
    private readonly double _weightRecentClicks = 1.0;
    private readonly double _weightMidClicks = 0.7;
    private readonly double _weightOldClicks = 0.4;
    
    private readonly TimeSpan _recentThreshold = TimeSpan.FromDays(1);
    private readonly TimeSpan _oldThreshold = TimeSpan.FromDays(3);

    private readonly int _firstCategoryListingCount = 7;
    private readonly int _secondCategoryListingCount = 4;
    private readonly int _thirdCategoryListingCount = 2;
    
    public async Task<List<ListingBasicResponse>> Handle(GetUserDedicatedListingsQuery request, CancellationToken cancellationToken)
    {
        var userViews = await listingViewRepository.GetUserListingViewsAsync(request.UserId);

        var queryable = listingRepository.GetListingsQueryable();
        if (request.ListingType != null)
        {
            queryable = request.ListingType == ListingType.Product ? queryable.OfType<ProductListing>() : queryable.OfType<AuctionListing>();
        }
        
        if (!userViews.Any())
        {
            var topListings = await queryable
                .Where(l => l.OwnerId != request.UserId)
                .OrderByDescending(l => l.ViewsCount)
                .ThenByDescending(l => l.DateCreated)
                .Take(_firstCategoryListingCount + _secondCategoryListingCount + _thirdCategoryListingCount)
                .ToListAsync(cancellationToken);
        
            var mappedListings = mapper.Map<List<ListingBasicResponse>>(topListings);
            
            if (request.UserId != null)
            {
                mappedListings = await SetIsFollowedField(mappedListings, request.UserId.Value);
            }
            return mappedListings;
        }
        
        var currentDate = DateTime.UtcNow;
        
        var categoriesWithPoints = userViews
            .GroupBy(uv => uv.RootCategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Points = g.Sum(view => GetWeightForView(view.ViewedAt, currentDate))
            })
            .OrderByDescending(c => c.Points)
            .Take(3)
            .ToList();

        foreach (var category in categoriesWithPoints)
        {
            logger.LogInformation($"Category: {category.CategoryId} - {category.Points} points");
        }
        
        var topThreeCategoryIds = categoriesWithPoints
            .Select(c => c.CategoryId)
            .ToList();
        
        var categoryLimits = GetCategoryLimits(topThreeCategoryIds.Count);
        
        var allPotentialListings = await queryable
            .Where(l => l.OwnerId != request.UserId && l.Categories.Any(c => topThreeCategoryIds.Contains(c.Id)))
            .OrderByDescending(l => l.ViewsCount)
            .ThenByDescending(l => l.DateCreated)
            .ToListAsync(cancellationToken);
        
        var resultListings = new List<ListingBase>();

        for (int i = 0; i < topThreeCategoryIds.Count; i++)
        {
            var categoryId = topThreeCategoryIds[i];
            var limit = categoryLimits[i];
            
            var categoryListings = allPotentialListings
                .Where(l => l.Categories.Any(c => c.Id == categoryId))
                .Take(limit)
                .ToList();
           
            resultListings.AddRange(categoryListings);
        }
        
        var mappedResultListings = mapper.Map<List<ListingBasicResponse>>(resultListings);
        
        mappedResultListings = await SetIsFollowedField(mappedResultListings, request.UserId!.Value);
        
        return mappedResultListings;
        
    }
    
    private List<int> GetCategoryLimits(int categoryCount)
    {
        return categoryCount switch
        {
            1 => new List<int> { _firstCategoryListingCount + _secondCategoryListingCount + _thirdCategoryListingCount },
            2 => new List<int> { _firstCategoryListingCount + _thirdCategoryListingCount, _secondCategoryListingCount },
            3 => new List<int> { _firstCategoryListingCount, _secondCategoryListingCount, _thirdCategoryListingCount },
            _ => new List<int>()
        };
    }
    
    private double GetWeightForView(DateTime viewedAt, DateTime now)
    {
        var diff = now - viewedAt;

        if (diff <= _recentThreshold)         
            return _weightRecentClicks;

        if (diff <= _oldThreshold)            
            return _weightMidClicks;

        return _weightOldClicks;                    
    }
    
    private async Task<List<ListingBasicResponse>> SetIsFollowedField(List<ListingBasicResponse> responses, int userId)
    {
        var followedListings = await listingRepository.GetUserFollowedListingsAsync(userId);
        foreach (var response in responses)
        {
            response.IsFollowed = followedListings.Select(f => f.ListingId).Contains(response.Id);
        }
        return responses;
    }
}