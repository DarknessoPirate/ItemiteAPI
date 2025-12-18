using Application.Features.Categories.CreateCategory;
using Application.Features.Categories.DeleteCategory;
using Application.Features.Categories.GetAllCategories;
using Application.Features.Categories.GetCategoryTree;
using Application.Features.Categories.GetMainCategories;
using Application.Features.Categories.GetSubCategories;
using Application.Features.Categories.UpdateCategory;
using Domain.DTOs.Category;
using Domain.DTOs.File;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ISender mediator) : ControllerBase
{
    
    // TODO : PROBABL REMOVE OR PRIVATE LATER WHEN NOT NEEDED
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

    // TODO: PROBABLY REMOVE OR PRIVATE LATER 
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

    [HttpGet("tree/{rootCategoryId:int}")]
    public async Task<ActionResult<List<CategoryTreeResponse>>> GetCategoryTree(int rootCategoryId)
    {
        var command = new GetCategoryTreeCommand
        {
            RootCategoryId = rootCategoryId
        };

        var result = await mediator.Send(command);
        
        return Ok(result);
    }
    
}