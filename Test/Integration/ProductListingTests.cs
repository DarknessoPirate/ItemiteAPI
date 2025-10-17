using Application.Exceptions;
using Application.Features.ProductListings.CreateProductListing;
using Application.Features.ProductListings.DeleteProductListing;
using Application.Features.ProductListings.GetPaginatedProductListings;
using Application.Features.ProductListings.GetProductListing;
using Domain.DTOs.File;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using Test.Configs;
using Xunit;

namespace Test.Integration;

public class ProductListingTests : BaseIntegrationTest, IAsyncLifetime
{
    
    public ProductListingTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateProductListing_ShouldThrow_NotFoundException()
    {
        var createCommand = new CreateProductListingCommand
        {
            ProductListingDto = new CreateProductListingRequest
            {
                CategoryId = Int32.MaxValue,
                Description = "test_listing",
                ImageOrders = [1],
                Location = "test_location",
                Name = "test_name",
                Price = 22.50M
            },
            Images = [new FileWrapper("test_image", 1000, "jpg", Stream.Null)],
            UserId = InitialUsers.First().Id
        };
        
        await Assert.ThrowsAsync<NotFoundException>(() => Sender.Send(createCommand));
        
    }
    
    [Fact]
    public async Task CreateProductListing_ShouldThrow_BadRequestException()
    {
        var createCommand = new CreateProductListingCommand
        {
            ProductListingDto = new CreateProductListingRequest
            {
                CategoryId = InitialCategories.First(c => c.RootCategoryId == null).Id,
                Description = "test_listing",
                ImageOrders = [1],
                Location = "test_location",
                Name = "test_name",
                Price = 22.50M
            },
            Images = [new FileWrapper("test_image", 1000, "jpg", Stream.Null)],
            UserId = InitialUsers.First().Id
        };
        
        await Assert.ThrowsAsync<BadRequestException>(() => Sender.Send(createCommand));
        
    }
    
    [Theory]
    [InlineData("", "test_location", "test_name", 20.50)]
    [InlineData("test", "test_location", "t", 20.50)]
    [InlineData("test", "test_location", "test_name", -20.50)]
    public async Task CreateProductListing_ShouldThrow_ValidatorException(string description, string location, string name, double price)
    {
        var createCommand = new CreateProductListingCommand
        {
            ProductListingDto = new CreateProductListingRequest
            {
                CategoryId = InitialCategories.First(c => c.RootCategoryId == null).Id,
                Description = description,
                ImageOrders = [1],
                Location = location,
                Name = name,
                Price = (decimal)price
            },
            Images = [new FileWrapper("test_image", 1000, "jpg", Stream.Null)],
            UserId = InitialUsers.First().Id
        };
        
        await Assert.ThrowsAsync<ValidatorException>(() => Sender.Send(createCommand));
        
    }
    
    [Fact]
    public async Task CreateProductListing_Should_CreateNewProductListing()
    {
        var createCommand = new CreateProductListingCommand
        {
            ProductListingDto = new CreateProductListingRequest
            {
                CategoryId = InitialCategories.First(c => c.Name == "Pants").Id,
                Description = "test_listing",
                ImageOrders = [1,2],
                Location = "test_location",
                Name = "test_name",
                Price = 100.50M
            },
            Images = [
                new FileWrapper("test_image1", 1000, "jpg", Stream.Null),
                new FileWrapper("test_image2", 1000, "jpg", Stream.Null)
            ],
            UserId = InitialUsers.First().Id
        };
        
        var createdProductListingId = await Sender.Send(createCommand);
        
        var createdListing = DbContext.Listings
            .Include(l => l.ListingPhotos)
            .ThenInclude(p => p.Photo)
            .FirstOrDefault(l => l.Id == createdProductListingId);
        createdListing.Should().NotBeNull();
        createdListing.Id.Should().Be(createdProductListingId);
        createdListing.Name.Should().Be("test_name");
        createdListing.OwnerId.Should().Be(InitialUsers.First().Id);
        createdListing.ListingPhotos.First().Photo.Url.Should().Be("https://fake-cloudinary-url.com/test_image1");
        createdListing.ListingPhotos.Last().Photo.Url.Should().Be("https://fake-cloudinary-url.com/test_image2");
    }


    [Fact]
    public async Task DeleteProductListing_ShouldThrow_ForbiddenException()
    {
        var listingIds = await AddTestProductListings();

        var deleteCommand = new DeleteProductListingCommand
        {
            ListingId = listingIds[0],
            UserId = InitialUsers.Last().Id
        };
        
        await Assert.ThrowsAsync<ForbiddenException>(() => Sender.Send(deleteCommand));
        
    }
    
    [Fact]
    public async Task DeleteProductListing_ShouldThrow_NotFoundException()
    {
        await AddTestProductListings();

        var deleteCommand = new DeleteProductListingCommand
        {
            ListingId = 999999,
            UserId = InitialUsers.Last().Id
        };
        
        await Assert.ThrowsAsync<NotFoundException>(() => Sender.Send(deleteCommand));
        
    }
    
    [Fact]
    public async Task DeleteProductListing_Should_DeleteProductListing()
    {
        var listingIds = await AddTestProductListings();

        var deleteCommand = new DeleteProductListingCommand
        {
            ListingId = listingIds[0],
            UserId = InitialUsers.First().Id
        };

        await Sender.Send(deleteCommand);

        var listings = await DbContext.Listings
            .Include(l => l.ListingPhotos)
            .ThenInclude(p => p.Photo).ToListAsync();
        
        var photos = await DbContext.Photos.ToListAsync();
        
        listings.Count.Should().Be(2);
        listings.FirstOrDefault(l => l.Id == listingIds[0]).Should().BeNull();
        photos.Count.Should().Be(4);
    }

    [Theory]
    [InlineData(-1,100,200)]
    [InlineData(101,100,200)]
    [InlineData(1,300,200)]
    public async Task GetPaginatedProductListing_ShouldThrow_ValidatorException(int pageSize, double priceFrom, double priceTo)
    {
        await AddTestProductListings();

        var query = new GetPaginatedProductListingsQuery
        {
            PageNumber = 1,
            PageSize = pageSize,
            PriceFrom = (decimal)priceFrom,
            PriceTo = (decimal)priceTo
        };
        
        await Assert.ThrowsAsync<ValidatorException>(() => Sender.Send(query));
    }
    
    [Fact]
    public async Task GetPaginatedProductListing_Should_ReturnCorrectPageResponse()
    {
        await AddTestProductListings();

        var query1 = new GetPaginatedProductListingsQuery
        {
            PageNumber = 1,
            PageSize = 2,
            SortBy = SortBy.Price,
            SortDirection = SortDirection.Ascending
        };
        
        var pageResponse1 = await Sender.Send(query1);
        
        pageResponse1.CurrentPage.Should().Be(1);
        pageResponse1.HasNextPage.Should().BeTrue();
        pageResponse1.TotalPages.Should().Be(2);
        pageResponse1.TotalItemsCount.Should().Be(3);
        pageResponse1.TotalPages.Should().Be(2);
        
        pageResponse1.Items.Should().HaveCount(2);
        pageResponse1.Items.First().Price.Should().Be(50.10M);
        pageResponse1.Items.First().Name.Should().Be("test_name2");
        pageResponse1.Items.First().MainImageUrl.Should().Be("https://fake-cloudinary-url.com/test_image1");
        
        var query2 = new GetPaginatedProductListingsQuery
        {
            PageNumber = 1,
            PageSize = 3,
            SortBy = SortBy.Price,
            SortDirection = SortDirection.Descending,
            PriceFrom = 300M,
            PriceTo = 6000M
        };
        
        var pageResponse2 = await Sender.Send(query2);
        
        pageResponse2.CurrentPage.Should().Be(1);
        pageResponse2.HasNextPage.Should().BeFalse();
        pageResponse2.TotalPages.Should().Be(1);
        pageResponse2.TotalItemsCount.Should().Be(1);
        pageResponse2.TotalPages.Should().Be(1);
        
        pageResponse2.Items.Should().HaveCount(1);
        pageResponse2.Items.First().Price.Should().Be(5000.50M);
        pageResponse2.Items.First().Name.Should().Be("test_name3");
        pageResponse2.Items.First().MainImageUrl.Should().Be("https://fake-cloudinary-url.com/test_image1");

    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public async Task GetProductListing_ShouldThrow_NotFoundException(int listingId)
    {
        await AddTestProductListings();

        var query = new GetProductListingQuery
        {
            ListingId = listingId
        };
        
        await Assert.ThrowsAsync<NotFoundException>(() => Sender.Send(query));
    }
    
    [Fact]
    public async Task GetProductListing_Should_ReturnCorrectProductListing()
    {
        var productListingIds = await AddTestProductListings();

        var query = new GetProductListingQuery
        {
            ListingId = productListingIds[0],
            UserId = InitialUsers.Last().Id
        };
        
        var productListing = await Sender.Send(query);
        
        productListing.Id.Should().Be(productListingIds[0]);
        productListing.Name.Should().Be("test_name1");
        productListing.MainImageUrl.Should().Be("https://fake-cloudinary-url.com/test_image1");
        productListing.Images.Should().HaveCount(2);
        productListing.IsFeatured.Should().BeFalse();
        productListing.IsNegotiable.Should().BeFalse();
        productListing.Categories.Should().HaveCount(3);
        productListing.Categories.Should().Contain(c => c.Name == "Clothing");
        productListing.Categories.Should().Contain(c => c.Name == "Pants");
        productListing.Owner.Id.Should().Be(InitialUsers.First().Id);
        productListing.Views.Should().Be(1);
    }
    
    private async Task<List<int>> AddTestProductListings()
    {
        var createCommand1 = new CreateProductListingCommand
        {
            ProductListingDto = new CreateProductListingRequest
            {
                CategoryId = InitialCategories.First(c => c.Name == "Pants").Id,
                Description = "test_listing1",
                ImageOrders = [1,2],
                Location = "test_location1",
                Name = "test_name1",
                Price = 100.50M
            },
            Images = [
                new FileWrapper("test_image1", 1000, "jpg", Stream.Null),
                new FileWrapper("test_image2", 1000, "jpg", Stream.Null)
            ],
            UserId = InitialUsers.First().Id
        };
        
        var createCommand2 = new CreateProductListingCommand
        {
            ProductListingDto = new CreateProductListingRequest
            {
                CategoryId = InitialCategories.First(c => c.Name == "Smartphones").Id,
                Description = "test_listing2",
                ImageOrders = [1],
                Location = "test_location2",
                Name = "test_name2",
                Price = 50.10M
            },
            Images = [
                new FileWrapper("test_image1", 1000, "jpg", Stream.Null),
            ],
            UserId = InitialUsers.First().Id
        };
        
        var createCommand3 = new CreateProductListingCommand
        {
            ProductListingDto = new CreateProductListingRequest
            {
                CategoryId = InitialCategories.First(c => c.Name == "Desktops").Id,
                Description = "test_listing3",
                ImageOrders = [1,2,3],
                Location = "test_location3",
                Name = "test_name3",
                Price = 5000.50M
            },
            Images = [
                new FileWrapper("test_image1", 1000, "jpg", Stream.Null),
                new FileWrapper("test_image2", 1000, "jpg", Stream.Null),
                new FileWrapper("test_image3", 1000, "jpg", Stream.Null)
            ],
            UserId = InitialUsers.Last().Id
        };
        
        var newListingId1 = await Sender.Send(createCommand1);
        var newListingId2 = await Sender.Send(createCommand2);
        var newListingId3 = await Sender.Send(createCommand3);
        
        return [newListingId1, newListingId2, newListingId3];
    }
    
    public Task InitializeAsync()
    {
        var testListing = DbContext.Listings
            .Where(c => c.Name.StartsWith("test"))
            .ToList();
        var testPhotos = DbContext.Photos.ToList();
        DbContext.RemoveRange(testListing);
        DbContext.RemoveRange(testPhotos);
        DbContext.SaveChanges();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}