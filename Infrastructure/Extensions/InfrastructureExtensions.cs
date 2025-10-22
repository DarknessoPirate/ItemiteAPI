using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IListingRepository<ProductListing>, ListingRepository<ProductListing>>();
        services.AddScoped<IListingRepository<AuctionListing>, ListingRepository<AuctionListing>>();
        services.AddScoped<IListingRepository<ListingBase>, ListingRepository<ListingBase>>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis")
                                    ?? throw new InvalidOperationException("Connection 'Redis' not found.");
            options.InstanceName = "itemite_";
        });
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = configuration.GetConnectionString("Redis")
                                   ?? throw new InvalidOperationException("Connection 'Redis' not found.");
            return ConnectionMultiplexer.Connect(connectionString);
        });
        
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>(); 
        services.AddHttpContextAccessor(); // to access user-agent/ip address/device id in controllers easier
        services.AddScoped<IRequestContextService, RequestContextService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
    }
}