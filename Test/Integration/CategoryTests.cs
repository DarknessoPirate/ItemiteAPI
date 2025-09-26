using Application.Exceptions;
using Application.Features.Categories.CreateCategory;
using Application.Features.Categories.GetAllCategories;
using Application.Features.Categories.GetMainCategories;
using Application.Features.Categories.GetSubCategories;
using Domain.DTOs.Category;
using Domain.Entities;
using FluentAssertions;
using Test.Configs;
using Xunit;

namespace Test.Integration;

public class CategoryTests : BaseIntegrationTest, IAsyncLifetime
{
    public CategoryTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("", "test", "test.jpg", null)]
    [InlineData("test", "", "test.jpg", 10)]
    [InlineData("x", "test", "test.jpg", null)]
    public async Task CreateCategory_ShouldThrow_ValidatorException(string name, string description, string imageUrl, int? parentCategoryId)
    {
        var command = new CreateCategoryCommand()
        {
            CreateCategoryDto = new CreateCategoryRequest()
            {
                Name = name,
                Description = description,
                ImageUrl = imageUrl,
                ParentCategoryId = parentCategoryId
            }
        };
        
        await Assert.ThrowsAsync<ValidatorException>(() => Sender.Send(command));
    }

    [Theory]
    [InlineData("test1", "test", "test.jpg", null)]
    [InlineData("test2", "test", "test.jpg", null)]
    [InlineData("test3", "test", "test.jpg", null)]
    public async Task CreateCategory_ShouldAddCategoryToDatabase(string name, string description, string imageUrl, int? parentCategoryId)
    {
        var command = new CreateCategoryCommand()
        {
            CreateCategoryDto = new CreateCategoryRequest()
            {
                Name = name,
                Description = description,
                ImageUrl = imageUrl,
                ParentCategoryId = parentCategoryId
            }
        };
        
        int createdCategoryId = await Sender.Send(command);
        var categoty = DbContext.Categories.FirstOrDefault(c => c.Id == createdCategoryId);
        categoty.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturn_AllCategories()
    {
        var addedCateogoriesIds = await AddTestCategories();

        var getAllCategoriesCommand = new GetAllCategoriesCommand();
        var allCategories = await Sender.Send(getAllCategoriesCommand);
        
        allCategories.Should().NotBeNull();
        allCategories.Should().NotBeEmpty();
        allCategories.Should().HaveCount(3);
        allCategories.First(c => c.Id == addedCateogoriesIds[0]).Name.Should().Be("test1");
        allCategories.First(c => c.Id == addedCateogoriesIds[1]).Name.Should().Be("test2");
        allCategories.First(c => c.Id == addedCateogoriesIds[2]).Name.Should().Be("test3");
    }

    
    [Fact]
    public async Task GetMainCategories_ShouldReturn_MainCategories()
    {
        var addedCateogoriesIds = await AddTestCategories();

        var getMainCategoriesCommand = new GetMainCategoriesCommand();
        var mainCategories = await Sender.Send(getMainCategoriesCommand);
        
        mainCategories.Should().NotBeNull();
        mainCategories.Should().NotBeEmpty();
        mainCategories.Should().HaveCount(2);
        mainCategories.First(c => c.Id == addedCateogoriesIds[0]).Name.Should().Be("test1");
        mainCategories.First(c => c.Id == addedCateogoriesIds[1]).Name.Should().Be("test2");
        mainCategories.FirstOrDefault(c => c.Id == addedCateogoriesIds[2]).Should().BeNull();
    }
    
    [Fact]
    public async Task GetSubCategories_ShouldReturn_SubCategories()
    {
        var addedCateogoriesIds = await AddTestCategories();

        var getSubCategoriesCommand = new GetSubCategoriesCommand()
        {
            ParentCategoryId = addedCateogoriesIds[0]
        };
        var subCategories = await Sender.Send(getSubCategoriesCommand);
        
        subCategories.Should().NotBeNull();
        subCategories.Should().NotBeEmpty();
        subCategories.Should().HaveCount(1);
        subCategories.FirstOrDefault(c => c.Id == addedCateogoriesIds[0]).Should().BeNull();
        subCategories.FirstOrDefault(c => c.Id == addedCateogoriesIds[1]).Should().BeNull();
        subCategories.First(c => c.Id == addedCateogoriesIds[2]).Name.Should().Be("test3");
    }
    
    [Fact]
    public async Task Category_ComplexCacheTest()
    {
        var addedCategoryIds = await AddTestCategories();

        var getAllCommand = new GetAllCategoriesCommand();
        var getMainCommand = new GetMainCategoriesCommand();
        var getSubCommand = new GetSubCategoriesCommand()
        {
            ParentCategoryId = addedCategoryIds[0]
        };
        
        // add cache for all categories
        var allCategoriesFromDb = await Sender.Send(getAllCommand);
        var allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>("all_categories");
        
        // add cache for main categories
        var mainCategoriesFromDb = await Sender.Send(getMainCommand);
        var mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>("main_categories");
        
        // add cache for sub categories
        var subCategoriesFromDb = await Sender.Send(getSubCommand);
        var subCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>($"sub_categories_{getSubCommand.ParentCategoryId}");
        
        allCategoriesFromCache.Should().NotBeNull();
        allCategoriesFromCache.Count.Should().Be(3);
        allCategoriesFromDb.Should().BeEquivalentTo(allCategoriesFromCache);
        
        mainCategoriesFromCache.Should().NotBeNull();
        mainCategoriesFromCache.Count.Should().Be(2);
        mainCategoriesFromDb.Should().BeEquivalentTo(mainCategoriesFromCache);
        
        subCategoriesFromCache.Should().NotBeNull();
        subCategoriesFromCache.Count.Should().Be(1);
        subCategoriesFromDb.Should().BeEquivalentTo(subCategoriesFromCache);
        
        var newMainCategoryCommand = new CreateCategoryCommand()
        {
            CreateCategoryDto = new CreateCategoryRequest()
            {
                Name = "test4",
                Description = "test",
                ImageUrl = "test.jpg",
            }
        };
        
        // should remove cache for keys "all_categories" and "main_categories"
        await Sender.Send(newMainCategoryCommand);
        
        allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>("all_categories");
        mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>("main_categories");
        subCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>($"sub_categories_{getSubCommand.ParentCategoryId}");
        
        allCategoriesFromCache.Should().BeNull();
        mainCategoriesFromCache.Should().BeNull();
        subCategoriesFromCache.Should().NotBeNull();
        
        // add cache again
        await Sender.Send(getAllCommand);
        await Sender.Send(getMainCommand);
        
        allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>("all_categories");
        mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>("main_categories");
        
        allCategoriesFromCache.Should().NotBeNull();
        mainCategoriesFromCache.Should().NotBeNull();
        
        var newSubCategoryCommand = new CreateCategoryCommand()
        {
            CreateCategoryDto = new CreateCategoryRequest()
            {
                Name = "test5",
                Description = "test",
                ImageUrl = "test.jpg",
                ParentCategoryId = getSubCommand.ParentCategoryId
            }
        };
        
        // should remove cache for keys "all_categories" and "sub_categories_{parrentId}"
        await Sender.Send(newSubCategoryCommand);
        
        allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>("all_categories");
        mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>("main_categories");
        subCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>($"sub_categories_{getSubCommand.ParentCategoryId}");
        
        allCategoriesFromCache.Should().BeNull();
        mainCategoriesFromCache.Should().NotBeNull();
        subCategoriesFromCache.Should().BeNull();

    }
    
    private async Task<List<int>> AddTestCategories()
    {
        var createCategoryDto1 = new CreateCategoryRequest()
        {
            Name = "test1",
            Description = "test1",
            ImageUrl = "test1.jpg",
        };
        
        var createCategoryDto2 = new CreateCategoryRequest()
        {
            Name = "test2",
            Description = "test2",
            ImageUrl = "test2.jpg",
        };
        
        
        var command1 = new CreateCategoryCommand() {CreateCategoryDto = createCategoryDto1};
        var command2 = new CreateCategoryCommand() {CreateCategoryDto = createCategoryDto2};
        
        int categoryId1 = await Sender.Send(command1);
        int categoryId2 = await Sender.Send(command2);
        
        var createCategoryDto3 = new CreateCategoryRequest()
        {
            Name = "test3",
            Description = "test3",
            ImageUrl = "test3.jpg",
            ParentCategoryId = categoryId1
        };
        
        var command3 = new CreateCategoryCommand() {CreateCategoryDto = createCategoryDto3};
        int categoryId3 = await Sender.Send(command3);
        
        return [categoryId1, categoryId2, categoryId3];
    }

    // execute before every test method
    public Task InitializeAsync()
    {
        ClearTable<Category>();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}