using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Listing;
using Domain.DTOs.Pagination;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Listings.Shared.GetPaginatedListings;

public class GetPaginatedListingsHandler(
    IListingRepository<ListingBase> repository,
    IMapper mapper,
    ICacheService cacheService
    ) : IRequestHandler<GetPaginatedListingsQuery, PageResponse<ListingBasicResponse>>
{
    public async Task<PageResponse<ListingBasicResponse>> Handle(GetPaginatedListingsQuery request, CancellationToken cancellationToken)
    {
        var cachedListings = await cacheService.GetAsync<PageResponse<ListingBasicResponse>>(
            $"{CacheKeys.LISTINGS}{request}");

        if (cachedListings != null)
        {
            return cachedListings;
        }

        PageResponse<ListingBasicResponse> pageResponse;

        if (request.ListingType == ListingType.Product)
        {
            pageResponse = await HandleProductListings(request, cancellationToken);
        }
        else if (request.ListingType == ListingType.Auction)
        {
            pageResponse = await HandleAuctionListings(request, cancellationToken);
        }
        else
        {
            pageResponse = await HandleBothListingTypes(request, cancellationToken);
        }

        await cacheService.SetAsync($"{CacheKeys.LISTINGS}{request}", pageResponse);
        return pageResponse;
    }
    
    private async Task<PageResponse<ListingBasicResponse>> HandleProductListings(
        GetPaginatedListingsQuery request, CancellationToken cancellationToken)
    {
        var queryable = repository.GetListingsQueryable().OfType<ProductListing>();
        
        queryable = FilterByCategories(queryable, request.CategoryIds);
        queryable = FilterProductByPrice(queryable, request.PriceFrom, request.PriceTo);
        queryable = FilterByDistance(queryable, request.Longitude, request.Latitude, request.Distance);
        queryable = SortProductListings(queryable, request.SortBy, request.SortDirection);

        int totalItems = await queryable.CountAsync(cancellationToken);
        
        queryable = queryable
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        var listings = await queryable.ToListAsync(cancellationToken);
        var responses = mapper.Map<List<ListingBasicResponse>>(listings);

        return new PageResponse<ListingBasicResponse>(responses, totalItems, request.PageSize, request.PageNumber);
    }

    private async Task<PageResponse<ListingBasicResponse>> HandleAuctionListings(
        GetPaginatedListingsQuery request, CancellationToken cancellationToken)
    {
        var queryable = repository.GetListingsQueryable().OfType<AuctionListing>();
        
        queryable = FilterByCategories(queryable, request.CategoryIds);
        queryable = FilterAuctionByPrice(queryable, request.PriceFrom, request.PriceTo);
        queryable = FilterByDistance(queryable, request.Longitude, request.Latitude, request.Distance);
        queryable = SortAuctionListings(queryable, request.SortBy, request.SortDirection);

        int totalItems = await queryable.CountAsync(cancellationToken);
        
        queryable = queryable
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        var listings = await queryable.ToListAsync(cancellationToken);
        var responses = mapper.Map<List<ListingBasicResponse>>(listings);

        return new PageResponse<ListingBasicResponse>(responses, totalItems, request.PageSize, request.PageNumber);
    }

    private async Task<PageResponse<ListingBasicResponse>> HandleBothListingTypes(
        GetPaginatedListingsQuery request, CancellationToken cancellationToken)
    {
        var productQuery = repository.GetListingsQueryable().OfType<ProductListing>();
        var auctionQuery = repository.GetListingsQueryable().OfType<AuctionListing>();

        productQuery = FilterByCategories(productQuery, request.CategoryIds);
        auctionQuery = FilterByCategories(auctionQuery, request.CategoryIds);

        productQuery = FilterProductByPrice(productQuery, request.PriceFrom, request.PriceTo);
        auctionQuery = FilterAuctionByPrice(auctionQuery, request.PriceFrom, request.PriceTo);
        
        productQuery = FilterByDistance(productQuery, request.Longitude, request.Latitude , request.Distance);
        auctionQuery = FilterByDistance(auctionQuery, request.Longitude, request.Latitude , request.Distance);
        
        productQuery = SortProductListings(productQuery, request.SortBy, request.SortDirection);
        auctionQuery = SortAuctionListings(auctionQuery, request.SortBy, request.SortDirection);

        var products = await productQuery.ToListAsync(cancellationToken);
        var auctions = await auctionQuery.ToListAsync(cancellationToken);

        var allListings = new List<ListingBase>();
        allListings.AddRange(products);
        allListings.AddRange(auctions);
        
        allListings = SortCombinedListings(allListings, request.SortBy, request.SortDirection);

        int totalItems = allListings.Count;
        
        var paginatedListings = allListings
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var responses = mapper.Map<List<ListingBasicResponse>>(paginatedListings);

        return new PageResponse<ListingBasicResponse>(responses, totalItems, request.PageSize, request.PageNumber);
    }
    
    private IQueryable<T> FilterByCategories<T>(IQueryable<T> queryable, List<int>? categoryIds) 
        where T : ListingBase
    {
        if (categoryIds != null && categoryIds.Any())
        {
            queryable = queryable.Where(l => l.Categories.Any(c => categoryIds.Contains(c.Id)));
        }
        return queryable;
    }

    private IQueryable<ProductListing> FilterProductByPrice(
        IQueryable<ProductListing> queryable, decimal? priceFrom, decimal? priceTo)
    {
        if (priceFrom != null && priceTo != null)
        {
            queryable = queryable.Where(p => p.Price >= priceFrom && p.Price <= priceTo);
        }
        else if (priceFrom != null)
        {
            queryable = queryable.Where(p => p.Price >= priceFrom);
        }
        else if (priceTo != null)
        {
            queryable = queryable.Where(p => p.Price <= priceTo);
        }
        return queryable;
    }

    private IQueryable<AuctionListing> FilterAuctionByPrice(
        IQueryable<AuctionListing> queryable, decimal? priceFrom, decimal? priceTo)
    {
        if (priceFrom != null && priceTo != null)
        {
            queryable = queryable.Where(a => 
                (a.CurrentBid ?? a.StartingBid) >= priceFrom && 
                (a.CurrentBid ?? a.StartingBid) <= priceTo);
        }
        else if (priceFrom != null)
        {
            queryable = queryable.Where(a => (a.CurrentBid ?? a.StartingBid) >= priceFrom);
        }
        else if (priceTo != null)
        {
            queryable = queryable.Where(a => (a.CurrentBid ?? a.StartingBid) <= priceTo);
        }
        return queryable;
    }

    private IQueryable<T> FilterByDistance<T>(
        IQueryable<T> queryable, double? longitude, double? latitude, double? distance)
        where T : ListingBase
    {
        if (longitude != null && latitude != null )
        {
            double lat = latitude.Value;
            double lon = longitude.Value;
            double dist = distance ?? 0;

            queryable = queryable
                .Where(l => l.Location.Latitude.HasValue && l.Location.Longitude.HasValue)
                .Where(l =>
                    6371 *
                    (2 * Math.Asin(Math.Sqrt(
                        Math.Pow(Math.Sin((l.Location.Latitude.Value - lat) * Math.PI / 180 / 2), 2) +
                        Math.Cos(lat * Math.PI / 180) * Math.Cos(l.Location.Latitude.Value * Math.PI / 180) *
                        Math.Pow(Math.Sin((l.Location.Longitude.Value - lon) * Math.PI / 180 / 2), 2)
                    ))) <= dist
                );
        }

        return queryable;
    }

    private IQueryable<ProductListing> SortProductListings(
        IQueryable<ProductListing> queryable, SortBy? sortBy, SortDirection? sortDirection)
    {
        IOrderedQueryable<ProductListing> orderedQuery = queryable.OrderByDescending(p => p.IsFeatured);

        if (sortBy == SortBy.Price)
        {
            orderedQuery = sortDirection == SortDirection.Ascending
                ? orderedQuery.ThenBy(p => p.Price)
                : orderedQuery.ThenByDescending(p => p.Price);
        }
        else if (sortBy == SortBy.CreationDate)
        {
            orderedQuery = sortDirection == SortDirection.Ascending
                ? orderedQuery.ThenBy(p => p.DateCreated)
                : orderedQuery.ThenByDescending(p => p.DateCreated);
        }
        else if (sortBy == SortBy.Views)
        {
            orderedQuery = sortDirection == SortDirection.Ascending
                ? orderedQuery.ThenBy(p => p.Views)
                : orderedQuery.ThenByDescending(p => p.Views);
        }
        else
        {
            orderedQuery = orderedQuery.ThenByDescending(p => p.DateCreated);
        }

        return orderedQuery;
    }

    private IQueryable<AuctionListing> SortAuctionListings(
        IQueryable<AuctionListing> queryable, SortBy? sortBy, SortDirection? sortDirection)
    {
        IOrderedQueryable<AuctionListing> orderedQuery = queryable.OrderByDescending(a => a.IsFeatured);

        if (sortBy == SortBy.Price)
        {
            orderedQuery = sortDirection == SortDirection.Ascending
                ? orderedQuery.ThenBy(a => a.CurrentBid ?? a.StartingBid)
                : orderedQuery.ThenByDescending(a => a.CurrentBid ?? a.StartingBid);
        }
        else if (sortBy == SortBy.CreationDate)
        {
            orderedQuery = sortDirection == SortDirection.Ascending
                ? orderedQuery.ThenBy(a => a.DateCreated)
                : orderedQuery.ThenByDescending(a => a.DateCreated);
        }
        else if (sortBy == SortBy.Views)
        {
            orderedQuery = sortDirection == SortDirection.Ascending
                ? orderedQuery.ThenBy(a => a.Views)
                : orderedQuery.ThenByDescending(a => a.Views);
        }
        else
        {
            orderedQuery = orderedQuery.ThenByDescending(a => a.DateCreated);
        }

        return orderedQuery;
    }

    private List<ListingBase> SortCombinedListings(
        List<ListingBase> listings, SortBy? sortBy, SortDirection? sortDirection)
    {
        var sorted = listings.OrderByDescending(l => l.IsFeatured);

        if (sortBy == SortBy.Price)
        {
            sorted = sortDirection == SortDirection.Ascending
                ? sorted.ThenBy(l => GetPriceForSorting(l))
                : sorted.ThenByDescending(l => GetPriceForSorting(l));
        }
        else if (sortBy == SortBy.CreationDate)
        {
            sorted = sortDirection == SortDirection.Ascending
                ? sorted.ThenBy(l => l.DateCreated)
                : sorted.ThenByDescending(l => l.DateCreated);
        }
        else if (sortBy == SortBy.Views)
        {
            sorted = sortDirection == SortDirection.Ascending
                ? sorted.ThenBy(l => l.Views)
                : sorted.ThenByDescending(l => l.Views);
        }
        else
        {
            sorted = sorted.ThenByDescending(l => l.DateCreated);
        }

        return sorted.ToList();
    }

    private decimal GetPriceForSorting(ListingBase listing)
    {
        return listing switch
        {
            ProductListing product => product.Price,
            AuctionListing auction => auction.CurrentBid ?? auction.StartingBid,
            _ => 0
        };
    }
    
    // private double Distance(double lattitude1, double longitude1, double lattitude2, double longitude2)
    // {
    //     var r = 6371.0;
    //     var dLat = ToRadians(lattitude2 - lattitude1);
    //     var dLon = ToRadians(longitude2 - longitude1);
    //     var a = Math.Sin(dLat/2.0) * Math.Sin(dLat/2.0) + Math.Cos(ToRadians(lattitude1)) * Math.Cos(ToRadians(lattitude2)) * Math.Sin(dLon/2.0) * Math.Sin(dLon/2.0);
    //     var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0-a));
    //     return r * c;
    // }
    //
    // private double ToRadians(double angle) => angle * Math.PI / 180.0;
}