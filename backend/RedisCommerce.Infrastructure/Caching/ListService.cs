using RedisCommerce.Application.Interfaces;
using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Caching;

public class ListService : RedisServiceBase, IListService
{
    public ListService(IConnectionMultiplexer connectionMultiplexer)
        : base(connectionMultiplexer)
    {
    }

    public async Task<long> LeftPushAsync(string key, string value) =>
        await Database.ListLeftPushAsync(key, value);

    public async Task<long> RightPushAsync(string key, string value) =>
        await Database.ListRightPushAsync(key, value);

    public async Task<string?> LeftPopAsync(string key)
    {
        var value = await Database.ListLeftPopAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<string?> RightPopAsync(string key)
    {
        var value = await Database.ListRightPopAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<long> LengthAsync(string key) =>
        await Database.ListLengthAsync(key);
}
