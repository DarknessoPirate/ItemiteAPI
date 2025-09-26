using Infrastructure.Database;
using Infrastructure.Services.Caching;
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

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<ItemiteDbContext>();
        Cache = _scope.ServiceProvider.GetRequiredService<ICacheService>();
    }

    // remove all entities for passed entity class 
    protected void ClearTable<T>() where T : class
    {
        var entities = DbContext.Set<T>();
        DbContext.RemoveRange(entities);
        DbContext.SaveChanges();
    }
    
}