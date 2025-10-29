using Infrastructure.Database;
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
    }
}