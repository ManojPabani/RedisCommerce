using StackExchange.Redis;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Caching;

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    private IDatabase Database => _connectionMultiplexer.GetDatabase();

    public async Task<string?> GetAsync(string key)
    {
        var value = await Database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task SetAsync(string key, string value, TimeSpan expiration)
    {
        await Database.StringSetAsync(key, value, expiration);
    }

    public async Task RemoveAsync(string key)
    {
        await Database.KeyDeleteAsync(key);
    }

    public async Task<bool> RefreshExpirationAsync(string key, TimeSpan expiration)
    {
        return await Database.KeyExpireAsync(key, expiration);
    }
}
