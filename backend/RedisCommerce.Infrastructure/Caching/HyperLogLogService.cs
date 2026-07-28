using RedisCommerce.Application.Interfaces;
using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Caching;

public class HyperLogLogService : RedisServiceBase, IHyperLogLogService
{
    public HyperLogLogService(IConnectionMultiplexer connectionMultiplexer)
        : base(connectionMultiplexer)
    {
    }

    public async Task<bool> AddAsync(string key, string value) =>
        await Database.HyperLogLogAddAsync(key, value);

    public async Task<long> CountAsync(params string[] keys)
    {
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        return await Database.HyperLogLogLengthAsync(redisKeys);
    }

    public async Task MergeAsync(string destinationKey, params string[] sourceKeys)
    {
        var redisKeys = sourceKeys.Select(k => (RedisKey)k).ToArray();
        await Database.HyperLogLogMergeAsync(destinationKey, redisKeys);
    }
}
