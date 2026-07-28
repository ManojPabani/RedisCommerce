using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Interfaces;
using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Caching;

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
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
