using System.Security.Claims;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class RequestContextService : IRequestContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdClaim = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("User not authenticated");
        }

        return userId;
    }

    public int? GetUserIdNullable()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdClaim = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return string.IsNullOrEmpty(userIdClaim) ? null : int.Parse(userIdClaim);
    }

    public string GetIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return "Unknown";
        // check for forwarded ip (if behind proxy/load balancer)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    public string? GetUserAgent()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Request.Headers.UserAgent.FirstOrDefault();
    }

    public string? GetDeviceId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        // Device ID is typically sent by client in a custom header
        return
            httpContext?.Request.Headers["X-Device-Id"]
                .FirstOrDefault(); // the "X-Device-Id" is a custom header, we can rename it in the client app to whatever
        // TODO: check from client and implement if needed
    }
}