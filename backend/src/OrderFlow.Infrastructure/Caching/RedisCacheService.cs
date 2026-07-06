using System.Text.Json;
using OrderFlow.Application.Interfaces;
using StackExchange.Redis;

namespace OrderFlow.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db    = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, serialized, expiry ?? TimeSpan.FromMinutes(5));
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => await _db.KeyDeleteAsync(key);

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var server  = _redis.GetServer(_redis.GetEndPoints().First());
        var keys    = server.Keys(pattern: $"{prefix}*").ToArray();
        if (keys.Length > 0)
            await _db.KeyDeleteAsync(keys);
    }
}
