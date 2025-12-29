using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Api.Extensions;

public static class RateLimiterExtensions
{
    public static void AddRateLimiting(this IServiceCollection services)
    { 
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = 429;
            o.AddPolicy("CreateReportPolicy", context =>
            {
                var partitionKey = context.User.Identity?.IsAuthenticated == true
                    ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown"
                    : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });
            o.AddPolicy("LoginPolicy", context =>
            {
                var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });
        });
    }
}