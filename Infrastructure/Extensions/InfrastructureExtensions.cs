using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Repositories;
using Infrastructure.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ItemiteDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")
                              ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
        });
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis")
                                    ?? throw new InvalidOperationException("Connection 'Redis' not found.");
            options.InstanceName = "itemite_";
        });

        services.AddScoped<ICacheService, CacheService>();
    }
}