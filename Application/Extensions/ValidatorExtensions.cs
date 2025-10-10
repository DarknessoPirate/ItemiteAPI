using Application.Features.Auth.EmailConfirmation;
using Application.Features.Auth.Login;
using Application.Features.Auth.RefreshToken;
using Application.Features.Auth.Register;
using Application.Features.Auth.ResetPassword;
using Application.Features.Categories.CreateCategory;
using Application.Features.Categories.DeleteCategory;
using Application.Features.Categories.GetCategoryTree;
using Application.Features.Categories.UpdateCategory;
using Application.Features.ProductListings.GetPaginatedProductListings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ValidatorExtensions
{
    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateCategoryCommand>, CreateCategoryValidator>();
        services.AddScoped<IValidator<RegisterCommand>, RegisterValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginValidator>();
        services.AddScoped<IValidator<EmailConfirmationCommand>, EmailConfirmationValidator>();
        services.AddScoped<IValidator<ResetPasswordCommand>, ResetPasswordValidator>();
        services.AddScoped<IValidator<DeleteCategoryCommand>, DeleteCategoryValidator>();
        services.AddScoped<IValidator<GetCategoryTreeCommand>, GetCategoryTreeValidator>();
        services.AddScoped<IValidator<UpdateCategoryCommand>, UpdateCategoryValidator>();
        services.AddScoped<IValidator<GetPaginatedProductListingsQuery>, GetPaginatedProductListingsValidator>();
    }
}