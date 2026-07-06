using OrderFlow.Application.Interfaces;

namespace OrderFlow.Infrastructure.Caching;

/// <summary>
/// No-op cache service used when Redis is not configured (e.g., tests, local dev without Redis).
/// </summary>
public sealed class NullCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
