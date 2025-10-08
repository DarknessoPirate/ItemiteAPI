using Application.Exceptions;
using Application.Features.Categories.CreateCategory;
using Application.Features.Categories.DeleteCategory;
using Application.Features.Categories.GetAllCategories;
using Application.Features.Categories.GetCategoryTree;
using Application.Features.Categories.GetMainCategories;
using Application.Features.Categories.GetSubCategories;
using Application.Features.Categories.UpdateCategory;
using Domain.Configs;
using Domain.DTOs.Category;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Exceptions;
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
    [InlineData("test", "", "test.jpg", -1)]
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
    [InlineData("test1", "test", "test.jpg", 10)]
    [InlineData("test2", "test", "test.jpg", 20)]
    [InlineData("test3", "test", "test.jpg", 30)]
    public async Task CreateCategory_ShouldThrow_NotFoundException(string name, string description, string imageUrl, int? parentCategoryId)
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
        
        await Assert.ThrowsAsync<NotFoundException>(() => Sender.Send(command));
    }
    
    [Theory]
    [InlineData("test1", "test", "test.jpg", null)]
    [InlineData("test2", "test", "test.jpg", null)]
    [InlineData("test3", "test", "test.jpg",1)]
    public async Task CreateCategory_ShouldThrow_BadRequestException(string name, string description, string imageUrl, int? parentCategoryId)
    {
        await AddTestCategories();
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
        
        await Assert.ThrowsAsync<BadRequestException>(() => Sender.Send(command));
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
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task DeleteCategory_ShouldThrow_NotFoundException(int categoryId)
    {
        var command = new DeleteCategoryCommand()
        {
            CategoryId = categoryId
        };
        
        await Assert.ThrowsAsync<NotFoundException>(() => Sender.Send(command));
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(null)]
    [InlineData(-100)]
    public async Task DeleteCategory_ShouldThrow_ValidationException(int categoryId)
    {
        var command = new DeleteCategoryCommand()
        {
            CategoryId = categoryId
        };
        
        await Assert.ThrowsAsync<ValidatorException>(() => Sender.Send(command));
    }
    
    [Fact]
    public async Task DeleteCategory_ShouldThrow_BadRequestException()
    {
        var savedCategoriesIds = await AddTestCategories();
        var command = new DeleteCategoryCommand()
        {
            CategoryId = savedCategoriesIds[0]
        };
        
        await Assert.ThrowsAsync<BadRequestException>(() => Sender.Send(command));
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1)]
    [InlineData(40)]
    public async Task UpdateCategory_ShouldThrow_NotFoundException(int categoryId)
    {
        var command = new UpdateCategoryCommand()
        {
            CategoryId = categoryId,
            Dto = new UpdateCategoryRequest()
            {
                Name = "test",
                Description = "test",
                ImageUrl = "test.jpg",
                ParentCategoryId = null
            }
        };
        await Assert.ThrowsAsync<NotFoundException>(() => Sender.Send(command));
    }

    [Theory]
    [InlineData(1, "t", "test", "test.jpg", null)]
    [InlineData(-1, "test", "test", "test.jpg", null)]
    [InlineData(2, "", "test", "test.jpg", null)]
    public async Task UpdateCategory_ShouldThrow_ValidationException(int categoryId, string name, string description, string imageUrl, int? parentCategoryId)
    {
        var command = new UpdateCategoryCommand()
        {
            CategoryId = categoryId,
            Dto = new UpdateCategoryRequest()
            {
                Name = name,
                Description = description,
                ImageUrl = imageUrl,
                ParentCategoryId = parentCategoryId
            }
        };
        await Assert.ThrowsAsync<ValidatorException>(() => Sender.Send(command));
    }
    
    [Fact]
    public async Task UpdateCategory_ShouldThrow_BadRequestException()
    {
        var savedCategoriesIds = await AddTestCategories();
        var command = new UpdateCategoryCommand()
        {
            CategoryId = savedCategoriesIds[2],
            Dto = new UpdateCategoryRequest()
            {
                // same name as parent
                Name = "test1",
                Description = "test",
                ImageUrl = "image.jpg",
                ParentCategoryId = savedCategoriesIds[0]
            }
        };
        await Assert.ThrowsAsync<BadRequestException>(() => Sender.Send(command));
        
        var command2 = new UpdateCategoryCommand()
        {
            CategoryId = savedCategoriesIds[0],
            Dto = new UpdateCategoryRequest()
            {
                Name = "test1",
                Description = "test",
                ImageUrl = "image.jpg",
                // categoryId = ParentCategoryId 
                ParentCategoryId = savedCategoriesIds[0]
            }
        };
        await Assert.ThrowsAsync<BadRequestException>(() => Sender.Send(command2));
    }
    
    [Fact]
    public async Task UpdateCategory_ShouldUpdateCategoryInDatabase()
    {
        var savedCategoriesIds = await AddTestCategories();
        var command = new UpdateCategoryCommand()
        {
            CategoryId = savedCategoriesIds[2],
            Dto = new UpdateCategoryRequest()
            {
                Name = "test_modified",
                Description = "test_desc_modified",
                ImageUrl = "image_modified.jpg",
                ParentCategoryId = savedCategoriesIds[1]
            }
        };

        var updatedCategory = await Sender.Send(command);
        updatedCategory.Name.Should().Be("test_modified");
        updatedCategory.Description.Should().Be("test_desc_modified");
        updatedCategory.ImageUrl.Should().Be("image_modified.jpg");

        var getSubCategoriesCommand = new GetSubCategoriesCommand
        {
            ParentCategoryId = savedCategoriesIds[1]
        };
        var subCategories = await Sender.Send(getSubCategoriesCommand);
        subCategories.Select(c => c.Id).Contains(savedCategoriesIds[2])
            .Should().BeTrue();
        
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
    public async Task GetCategoryTree_ShouldReturn_CategoryTree()
    {
        var addedCateogoriesIds = await AddTestCategories();

        var getCategoryTreeCommand = new GetCategoryTreeCommand()
        {
            RootCategoryId = addedCateogoriesIds[0]
        };
        var categoryTree = await Sender.Send(getCategoryTreeCommand);
        
        categoryTree.Should().NotBeNull();
        categoryTree.Name.Should().Be("test1");
        categoryTree.SubCategories.First().Name.Should().Be("test3");
    }
    
    [Fact]
    public async Task CreateCategory_ComplexCacheTest()
    {
        var addedCategoryIds = await AddTestCategories();

        var getAllCommand = new GetAllCategoriesCommand();
        var getMainCommand = new GetMainCategoriesCommand();
        var getSubCommand = new GetSubCategoriesCommand()
        {
            ParentCategoryId = addedCategoryIds[0]
        };
        var getCategoryTreeCommand = new GetCategoryTreeCommand()
        {
            RootCategoryId = addedCategoryIds[0]
        };
        
        // add cache for all categories
        var allCategoriesFromDb = await Sender.Send(getAllCommand);
        var allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.ALL_CATEGORIES);
        
        // add cache for main categories
        var mainCategoriesFromDb = await Sender.Send(getMainCommand);
        var mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.MAIN_CATEGORIES);
        
        // add cache for sub categories
        var subCategoriesFromDb = await Sender.Send(getSubCommand);
        var subCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>($"{CacheKeys.SUB_CATEGORIES}{getSubCommand.ParentCategoryId}");
        
        // add cache for category tree
        var categoryTreeFromDb = await Sender.Send(getCategoryTreeCommand);
        var categoryTreeFromCache = await Cache.GetAsync<CategoryTreeResponse>($"{CacheKeys.CATEGORY_TREE}{getCategoryTreeCommand.RootCategoryId}");
        
        allCategoriesFromCache.Should().NotBeNull();
        allCategoriesFromCache.Count.Should().Be(3);
        allCategoriesFromDb.Should().BeEquivalentTo(allCategoriesFromCache);
        
        mainCategoriesFromCache.Should().NotBeNull();
        mainCategoriesFromCache.Count.Should().Be(2);
        mainCategoriesFromDb.Should().BeEquivalentTo(mainCategoriesFromCache);
        
        subCategoriesFromCache.Should().NotBeNull();
        subCategoriesFromCache.Count.Should().Be(1);
        subCategoriesFromDb.Should().BeEquivalentTo(subCategoriesFromCache);
        
        categoryTreeFromCache.Should().NotBeNull();
        categoryTreeFromDb.Should().BeEquivalentTo(categoryTreeFromCache);
        
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
        
        allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.ALL_CATEGORIES);
        mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.MAIN_CATEGORIES);
        subCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>($"{CacheKeys.SUB_CATEGORIES}{getSubCommand.ParentCategoryId}");
        categoryTreeFromCache = await Cache.GetAsync<CategoryTreeResponse>($"{CacheKeys.CATEGORY_TREE}{getCategoryTreeCommand.RootCategoryId}");
        
        allCategoriesFromCache.Should().BeNull();
        mainCategoriesFromCache.Should().BeNull();
        subCategoriesFromCache.Should().NotBeNull();
        categoryTreeFromCache.Should().NotBeNull();
        
        // add cache again
        await Sender.Send(getAllCommand);
        await Sender.Send(getMainCommand);
        
        allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.ALL_CATEGORIES);
        mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.MAIN_CATEGORIES);
        
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
        
        // should remove cache for keys "all_categories", "sub_categories_{parrentId} and "category_tree_{rootCategoryId}"
        await Sender.Send(newSubCategoryCommand);
        
        allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.ALL_CATEGORIES);
        mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.MAIN_CATEGORIES);
        subCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>($"{CacheKeys.SUB_CATEGORIES}{getSubCommand.ParentCategoryId}");
        categoryTreeFromCache = await Cache.GetAsync<CategoryTreeResponse>($"{CacheKeys.CATEGORY_TREE}{getCategoryTreeCommand.RootCategoryId}");
        
        allCategoriesFromCache.Should().BeNull();
        mainCategoriesFromCache.Should().NotBeNull();
        subCategoriesFromCache.Should().BeNull();
        categoryTreeFromCache.Should().BeNull();

    }
    
    [Fact]
    public async Task DeleteCategory_ComplexCacheTest()
    {
        var addedCategoryIds = await AddTestCategories();

        var getAllCommand = new GetAllCategoriesCommand();
        var getMainCommand = new GetMainCategoriesCommand();
        var getSubCommand = new GetSubCategoriesCommand()
        {
            ParentCategoryId = addedCategoryIds[0]
        };
        var getCategoryTreeCommand = new GetCategoryTreeCommand()
        {
            RootCategoryId = addedCategoryIds[0]
        };
        
        // add cache for all categories
        var allCategoriesFromDb = await Sender.Send(getAllCommand);
        var allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.ALL_CATEGORIES);
        
        // add cache for main categories
        var mainCategoriesFromDb = await Sender.Send(getMainCommand);
        var mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.MAIN_CATEGORIES);
        
        // add cache for sub categories
        var subCategoriesFromDb = await Sender.Send(getSubCommand);
        var subCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>($"{CacheKeys.SUB_CATEGORIES}{getSubCommand.ParentCategoryId}");
        
        // add cache for category tree
        var categoryTreeFromDb = await Sender.Send(getCategoryTreeCommand);
        var categoryTreeFromCache = await Cache.GetAsync<CategoryTreeResponse>($"{CacheKeys.CATEGORY_TREE}{getCategoryTreeCommand.RootCategoryId}");
        
        allCategoriesFromCache.Should().NotBeNull();
        allCategoriesFromCache.Count.Should().Be(3);
        allCategoriesFromDb.Should().BeEquivalentTo(allCategoriesFromCache);
        
        mainCategoriesFromCache.Should().NotBeNull();
        mainCategoriesFromCache.Count.Should().Be(2);
        mainCategoriesFromDb.Should().BeEquivalentTo(mainCategoriesFromCache);
        
        subCategoriesFromCache.Should().NotBeNull();
        subCategoriesFromCache.Count.Should().Be(1);
        subCategoriesFromDb.Should().BeEquivalentTo(subCategoriesFromCache);
        
        categoryTreeFromCache.Should().NotBeNull();
        categoryTreeFromDb.Should().BeEquivalentTo(categoryTreeFromCache);
        
        var deleteCategoryCommand = new DeleteCategoryCommand()
        {
            CategoryId = addedCategoryIds[2]
        };
        
        // should remove cache for keys "all_categories" and "sub_categories and category_tree"
        await Sender.Send(deleteCategoryCommand);
        
        allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.ALL_CATEGORIES);
        mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.MAIN_CATEGORIES);
        subCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>($"{CacheKeys.SUB_CATEGORIES}{getSubCommand.ParentCategoryId}");
        categoryTreeFromCache = await Cache.GetAsync<CategoryTreeResponse>($"{CacheKeys.CATEGORY_TREE}{getCategoryTreeCommand.RootCategoryId}");
        
        allCategoriesFromCache.Should().BeNull();
        mainCategoriesFromCache.Should().NotBeNull();
        subCategoriesFromCache.Should().BeNull();
        categoryTreeFromCache.Should().BeNull();
        
        // add cache again
        await Sender.Send(getAllCommand);
        await Sender.Send(getSubCommand);
        await Sender.Send(getCategoryTreeCommand);
        
        allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.ALL_CATEGORIES);
        subCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>($"{CacheKeys.SUB_CATEGORIES}{getSubCommand.ParentCategoryId}");
        categoryTreeFromCache = await Cache.GetAsync<CategoryTreeResponse>($"{CacheKeys.CATEGORY_TREE}{getCategoryTreeCommand.RootCategoryId}");
        
        allCategoriesFromCache.Should().NotBeNull();
        subCategoriesFromCache.Should().NotBeNull();
        categoryTreeFromCache.Should().NotBeNull();
        
        deleteCategoryCommand = new DeleteCategoryCommand()
        {
           CategoryId = addedCategoryIds[1]
        };
        
        // should remove cache for keys "all_categories", and main_categories"
        await Sender.Send(deleteCategoryCommand);
        
        allCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.ALL_CATEGORIES);
        mainCategoriesFromCache = await Cache.GetAsync<List<CategoryResponse>>(CacheKeys.MAIN_CATEGORIES);
        
        allCategoriesFromCache.Should().BeNull();
        mainCategoriesFromCache.Should().BeNull();
        subCategoriesFromCache.Should().NotBeNull();
        categoryTreeFromCache.Should().NotBeNull();

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