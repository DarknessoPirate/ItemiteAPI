using Domain.DTOs.Category;
using FluentValidation;
using Infrastructure.Interfaces.Repositories;

namespace Application.Features.Categories.CreateCategory;

public class CreateCategoryRequestValidator :  AbstractValidator<CreateCategoryRequest>
{
    
    private readonly ICategoryRepository _categoryRepository;
    
    public CreateCategoryRequestValidator(ICategoryRepository categoryRepository)
    {   
        _categoryRepository = categoryRepository;
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name cannot be empty")
            .Length(2, 50).WithMessage("Name must be between 2 and 50 characters")
            .MustAsync(IsUniqueName).WithMessage("Category name must be unique");

        RuleFor(x => x.Description)
            .MaximumLength(100).WithMessage("Description must be at max 100 characters");
        
        RuleFor(x => x.ParentCategoryId)
            .MustAsync(ParentCategoryExists)
            .When(x => x.ParentCategoryId.HasValue)
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