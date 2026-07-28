using RedisCommerce.Application.Interfaces;
using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Caching;

public class SetService : RedisServiceBase, ISetService
{
    public SetService(IConnectionMultiplexer connectionMultiplexer)
        : base(connectionMultiplexer)
    {
    }

    public async Task<bool> AddAsync(string key, string member) =>
        await Database.SetAddAsync(key, member);

    public async Task<bool> RemoveAsync(string key, string member) =>
        await Database.SetRemoveAsync(key, member);

    public async Task<IReadOnlyList<string>> MembersAsync(string key)
    {
        var members = await Database.SetMembersAsync(key);
        return members.Select(m => m.ToString()).ToList();
    }

    public async Task<bool> IsMemberAsync(string key, string member) =>
        await Database.SetContainsAsync(key, member);

    public async Task<long> CardinalityAsync(string key) =>
        await Database.SetLengthAsync(key);
}
