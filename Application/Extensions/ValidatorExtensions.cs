using Application.Features.Categories.CreateCategory;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ValidatorExtensions
{
    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateCategoryCommand>, CreateCategoryValidator>();
    }
}