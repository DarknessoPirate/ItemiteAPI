namespace Infrastructure.Interfaces.Services;

public interface IRequestContextService
{
   int GetUserId();
   int? GetUserIdNullable();
   public string? GetUsername();
   string GetIpAddress();
   string? GetUserAgent();
   string? GetDeviceId();
}