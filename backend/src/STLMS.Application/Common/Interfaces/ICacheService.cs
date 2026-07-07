namespace STLMS.Application.Common.Interfaces;

/// <summary>Redis-shaped cache abstraction. Backed by RedisCacheService in production and
/// MemoryCacheService as an automatic fallback when Redis isn't configured/reachable (which is
/// always true in local dev on this machine, since Redis isn't installed) - calling code never
/// knows or cares which one is active.</summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
