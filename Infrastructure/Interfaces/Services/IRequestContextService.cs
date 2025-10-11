namespace Infrastructure.Interfaces.Services;

public interface IRequestContextService
{
   int GetUserId();
   string GetIpAddress();
   string? GetUserAgent();
   string? GetDeviceId();
}