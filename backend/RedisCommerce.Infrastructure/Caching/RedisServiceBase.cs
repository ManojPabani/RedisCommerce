using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Caching;

public abstract class RedisServiceBase
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    protected RedisServiceBase(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    protected IDatabase Database => _connectionMultiplexer.GetDatabase();

    protected Task<bool> KeyExpireAsync(string key, TimeSpan expiration) =>
        Database.KeyExpireAsync(key, expiration);

    protected Task<bool> DeleteKeyAsync(string key) =>
        Database.KeyDeleteAsync(key);
}
