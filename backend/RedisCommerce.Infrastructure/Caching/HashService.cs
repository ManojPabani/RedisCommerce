using RedisCommerce.Application.Interfaces;
using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Caching;

public class HashService : RedisServiceBase, IHashService
{
    public HashService(IConnectionMultiplexer connectionMultiplexer)
        : base(connectionMultiplexer)
    {
    }

    public async Task HashSetAsync(string key, string field, string value)
    {
        await Database.HashSetAsync(key, field, value);
    }

    public async Task<string?> HashGetAsync(string key, string field)
    {
        var value = await Database.HashGetAsync(key, field);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<IDictionary<string, string>> HashGetAllAsync(string key)
    {
        var entries = await Database.HashGetAllAsync(key);
        return entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());
    }

    public async Task<bool> HashDeleteAsync(string key, string field) =>
        await Database.HashDeleteAsync(key, field);

    public async Task<bool> HashExistsAsync(string key, string field) =>
        await Database.HashExistsAsync(key, field);

    public async Task<long> HashLengthAsync(string key) =>
        await Database.HashLengthAsync(key);

    public Task<bool> ExpireAsync(string key, TimeSpan expiration) =>
        KeyExpireAsync(key, expiration);

    public Task<bool> DeleteAsync(string key) =>
        DeleteKeyAsync(key);
}
