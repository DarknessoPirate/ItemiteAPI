using FluentValidation;
using Infrastructure.Interfaces.Repositories;

namespace Application.Features.Categories.DeleteCategory;

public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
   public DeleteCategoryValidator(ICategoryRepository categoryRepository)
   {

      RuleFor(x => x.CategoryId)
         .NotNull().WithMessage("Category ID cannot be null")
         .GreaterThan(0).WithMessage("Category ID must be greater than 0");
   }
}