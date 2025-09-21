using Api.Middlewares;
using Serilog;

namespace Api.Extensions;

public static class ApiExtensions
{
    public static void AddApiServices(this IServiceCollection services)
    {
        services.AddExceptionHandler<AppExceptionHandler>();
        services.AddSingleton(Log.Logger);
    }
}