using Domain.DTOs.Category;
using FluentValidation;
using Infrastructure.Interfaces.Repositories;

namespace Application.Features.Categories.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    
    public CreateCategoryValidator(ICategoryRepository categoryRepository)
    {   
        _categoryRepository = categoryRepository;
        
        RuleFor(x => x.CreateCategoryDto.Name)
            .NotEmpty().WithMessage("Name cannot be empty")
            .Length(2, 50).WithMessage("Name must be between 2 and 50 characters")
            .MustAsync(IsUniqueName).WithMessage("Category name must be unique");

        RuleFor(x => x.CreateCategoryDto.Description)
            .MaximumLength(100).WithMessage("Description must be at max 100 characters");
        
        RuleFor(x => x.CreateCategoryDto.ParentCategoryId)
            .MustAsync(ParentCategoryExists)
            .When(x => x.CreateCategoryDto.ParentCategoryId.HasValue)
            .WithMessage("Parent category does not exist");


    }
    
    private async Task<bool> IsUniqueName(string name, CancellationToken cancellationToken)
    {
        return !await _categoryRepository.CategoryExistsByName(name);
    }
    
    private async Task<bool> ParentCategoryExists(int? parentId, CancellationToken cancellationToken)
    {
        return await _categoryRepository.CategoryExistsById(parentId!.Value); // .When() should not call this if the value is null 
    }
}