namespace Infrastructure.Interfaces.Services;

public interface IRequestContextService
{
   string GetIpAddress();
   string? GetUserAgent();
   string? GetDeviceId();
}