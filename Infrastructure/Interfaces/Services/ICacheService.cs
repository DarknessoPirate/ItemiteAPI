namespace Infrastructure.Interfaces.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, int cacheTimeInMinutes = 10);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}