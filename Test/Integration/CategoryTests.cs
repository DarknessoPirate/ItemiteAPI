using Application.Exceptions;
using Application.Features.Categories.CreateCategory;
using Domain.DTOs.Category;
using Domain.Entities;
using Test.Configs;
using Xunit;

namespace Test.Integration;

public class CategoryTests : BaseIntegrationTest
{
    public CategoryTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("", "test", "test.jpg", null)]
    [InlineData("test", "", "test.jpg", 1)]
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
        
        // this will execute 3 times for 3 inline data
        ClearTable<Category>();
        
        var categories = DbContext.Categories.ToList();
        Assert.Empty(categories);
    }
    
    // TODO: Add more tests
    
}