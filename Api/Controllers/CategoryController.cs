using Application.Features.Categories.CreateCategory;
using Domain.DTOs.Category;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ISender mediator) : ControllerBase
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
}