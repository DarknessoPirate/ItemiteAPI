using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Pagination;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductListings.GetPaginatedProductListings;

public class GetPaginatedProductListingHandler(
    IListingRepository<ProductListing> repository,
    IMapper mapper,
    ICacheService cacheService
    ) : IRequestHandler<GetPaginatedProductListingsQuery, PageResponse<ProductListingBasicResponse>>
{
    public async Task<PageResponse<ProductListingBasicResponse>> Handle(GetPaginatedProductListingsQuery request, CancellationToken cancellationToken)
    {
        var cachedListingProducts =
            await cacheService.GetAsync<PageResponse<ProductListingBasicResponse>>(
                $"{CacheKeys.PRODUCT_LISTINGS}{request}");

        if (cachedListingProducts != null)
        {
            return cachedListingProducts;
        }
        
        var queryableProductListings = repository.GetListingsQueryable();
        
        queryableProductListings = HandleProductListingFiltering(request, queryableProductListings);
        queryableProductListings = HandleProductListingSorting(request, queryableProductListings);
        
        int totalItems =  queryableProductListings.Count();
        int pageNumber = request.PageNumber;
        int pageSize = request.PageSize;

        queryableProductListings =  queryableProductListings.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var productListingResponseList =
            mapper.Map<List<ProductListingBasicResponse>>(await queryableProductListings.ToListAsync());
        
        var pageResponse = new PageResponse<ProductListingBasicResponse>(productListingResponseList, totalItems, pageSize, pageNumber);
        await cacheService.SetAsync($"{CacheKeys.PRODUCT_LISTINGS}{request}", pageResponse);
        
        return pageResponse;
    }

    private IQueryable<ProductListing> HandleProductListingFiltering(GetPaginatedProductListingsQuery request, IQueryable<ProductListing> queryableProductListings)
    {
        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            queryableProductListings =
                queryableProductListings.Where(p => p.Categories.Any(c => request.CategoryIds.Contains(c.Id)));
        }

        if (request.PriceFrom != null && request.PriceTo != null)
        {
            queryableProductListings = queryableProductListings.Where(p => p.Price >= request.PriceFrom && p.Price <= request.PriceTo);
        }
        else if (request.PriceFrom != null && request.PriceTo == null)
        {
            queryableProductListings = queryableProductListings.Where(p => p.Price >= request.PriceFrom);
        }
        else if (request.PriceFrom == null && request.PriceTo != null)
        {
            queryableProductListings = queryableProductListings.Where(p => p.Price <= request.PriceTo);
        }
        
        return queryableProductListings;
    }

    private IQueryable<ProductListing> HandleProductListingSorting(GetPaginatedProductListingsQuery request,
        IQueryable<ProductListing> queryableProductListings)
    {
        IOrderedQueryable<ProductListing> orderedQuery = 
            queryableProductListings.OrderByDescending(p => p.IsFeatured);
        
        if (request.SortBy == SortBy.Price)
        {
            orderedQuery = request.SortDirection == SortDirection.Ascending ? 
                orderedQuery.ThenBy(p => p.Price) :
                orderedQuery.ThenByDescending(p => p.Price);
        }
        else if (request.SortBy == SortBy.CreationDate)
        {
            orderedQuery = request.SortDirection == SortDirection.Ascending ? 
                orderedQuery.ThenBy(p => p.DateCreated) :
                orderedQuery.ThenByDescending(p => p.DateCreated);
        }
        else if (request.SortBy == SortBy.Views)
        {
            orderedQuery = request.SortDirection == SortDirection.Ascending ? 
                orderedQuery.ThenBy(p => p.Views) :
                orderedQuery.ThenByDescending(p => p.Views);
        }
        else
        {
            orderedQuery = orderedQuery.ThenByDescending(p => p.DateCreated);
        }
    
        return orderedQuery;
    }
}