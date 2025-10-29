namespace Infrastructure.Interfaces.Services;

public interface IRequestContextService
{
   int GetUserId();
   int? GetUserIdNullable();
   string GetIpAddress();
   string? GetUserAgent();
   string? GetDeviceId();
}