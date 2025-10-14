using Application.Features.ProductListings.CreateProductListing;
using Domain.DTOs.File;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Exceptions;
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
        
        var createdListing = DbContext.Listings.FirstOrDefault(l => l.Id == createdProductListingId);
        createdListing.Should().NotBeNull();
        createdListing.Id.Should().Be(createdProductListingId);
        createdListing.Name.Should().Be("test_name");
        createdListing.OwnerId.Should().Be(InitialUsers.First().Id);

        var photos = DbContext.Photos.ToList();
        photos.Should().HaveCount(2);
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