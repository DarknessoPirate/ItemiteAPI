using Domain.DTOs.Category;
using FluentValidation;
using Infrastructure.Interfaces.Repositories;

namespace Application.Features.Categories.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator(ICategoryRepository categoryRepository)
    {   
        RuleFor(x => x.CreateCategoryDto.Name)
            .NotEmpty().WithMessage("Name cannot be empty")
            .Length(2, 50).WithMessage("Name must be between 2 and 50 characters");
        RuleFor(x => x.CreateCategoryDto.PolishName)
            .NotEmpty().WithMessage("Name cannot be empty")
            .Length(2, 50).WithMessage("Name must be between 2 and 50 characters");

        RuleFor(x => x.CreateCategoryDto.Description)
            .MaximumLength(100).WithMessage("Description must be at max 100 characters");
        
        RuleFor(x => x.CreateCategoryDto.ParentCategoryId)
            .GreaterThan(0).WithMessage("Parent category id must be greater than 0");
    }
    
}