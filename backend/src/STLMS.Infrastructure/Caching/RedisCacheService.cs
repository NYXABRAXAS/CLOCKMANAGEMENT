using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using STLMS.Application.Common.Interfaces;

namespace STLMS.Infrastructure.Caching;

public class RedisCacheService(IDistributedCache cache) : ICacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(15);

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var raw = await cache.GetStringAsync(key, ct);
        return raw is null ? default : JsonSerializer.Deserialize<T>(raw);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var raw = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl ?? DefaultTtl };
        return cache.SetStringAsync(key, raw, options, ct);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default) => cache.RemoveAsync(key, ct);

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default) => await cache.GetStringAsync(key, ct) is not null;
}
