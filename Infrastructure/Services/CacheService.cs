using System.Text.Json;
using Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Infrastructure.Services;

public class CacheService(IDistributedCache cache, IConnectionMultiplexer redis) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key)
    {
        var data = await cache.GetStringAsync(key);
        return data == null ? default : JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(string key, T value, int cacheTimeInMinutes = 10)
    {
        var options = new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTimeInMinutes)
        };
        await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var endpoints = redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = redis.GetServer(endpoint);
            var keys = server.Keys(pattern: $"itemite_{pattern}");
            
            foreach (var key in keys)
            {
                var keyWithoutPrefix = key.ToString().Replace("itemite_", "");
                await cache.RemoveAsync(keyWithoutPrefix);
            }
        }
    }
}