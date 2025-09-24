using System.Reflection;
using Application.Behaviors;
using Application.Features.Categories.CreateCategory;
using Domain.DTOs.Category;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
        
        services.AddValidators(); // extension function to add fluent validators from "Features" subfolder
    }
}