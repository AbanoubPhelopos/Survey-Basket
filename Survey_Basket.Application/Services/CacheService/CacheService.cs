using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Survey_Basket.Application.Services.CacheService;

public class CacheService(IDistributedCache distributedCache) : ICacheService
{
    private readonly IDistributedCache _distributedCache = distributedCache;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var data = await _distributedCache.GetStringAsync(key, cancellationToken);

        return data is null ? null : JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
    {
        await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), cancellationToken);
    }


    public async Task RemoveAsyn(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }
}
