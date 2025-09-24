using Application.Features.Categories.CreateCategory;
using Application.Features.Categories.GetAllCategories;
using Application.Features.Categories.GetMainCategories;
using Application.Features.Categories.GetSubCategories;
using Domain.DTOs.Category;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
    {
        var command = new CreateCategoryCommand
        {
            CreateCategoryDto = request
        };

        await mediator.Send(command);

        return Ok();
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<CategoryResponse>>> GetAllCategories()
    {
        var command = new GetAllCategoriesCommand();
        var result = await mediator.Send(command);

        return Ok(result);
    }

    [HttpGet("main")]
    public async Task<ActionResult<List<CategoryResponse>>> GetMainCategories()
    {
        var command = new GetMainCategoriesCommand();
        var result = await mediator.Send(command);

        return Ok(result);
    }

    [HttpGet("sub/{parentId:int}")]
    public async Task<ActionResult<List<CategoryResponse>>> GetSubCategories(int parentId)
    {
        var command = new GetSubCategoriesCommand
        {
            ParentCategoryId = parentId
        };
        var result = await mediator.Send(command);
        
        return Ok(result);
    }
}