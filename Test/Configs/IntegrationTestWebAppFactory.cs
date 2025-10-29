using Api;
using Infrastructure.Database;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test.Mocks;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace Test.Configs;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("ItemiteDB")
        .WithUsername("user")
        .WithPassword("password")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .Build();
        

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false);
        });
        
        builder.ConfigureTestServices(services =>
        {
            RemoveService<DbContextOptions<ItemiteDbContext>>(services);
            RemoveService<ItemiteDbContext>(services);
            RemoveService<IDistributedCache>(services);
            RemoveService<IMediaService>(services);

            services.AddDbContext<ItemiteDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.EnableSensitiveDataLogging();
            });
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _redisContainer.GetConnectionString();
                options.InstanceName = "itemite_";
            });
            services.AddScoped<IMediaService, MediaServiceMock>();
        });
    }

    private void RemoveService<T>(IServiceCollection services)
    {
        var descriptors = services.Where(s => s.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private async Task SeedDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
        await seeder.SeedAsync();
    }
    
    private async Task RunMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ItemiteDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        await RunMigrationsAsync();
        
        await SeedDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
}