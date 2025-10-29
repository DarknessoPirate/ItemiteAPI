using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Test.Configs;

public class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IServiceScope _scope;
    
    protected readonly ISender Sender;
    protected readonly ItemiteDbContext DbContext;
    protected readonly ICacheService Cache;
    protected readonly List<User> InitialUsers;
    protected readonly List<Category> InitialCategories;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<ItemiteDbContext>();
        Cache = _scope.ServiceProvider.GetRequiredService<ICacheService>();
        
        InitialUsers = DbContext.Users.ToList();
        InitialCategories = DbContext.Categories.ToList();
    }
    
}