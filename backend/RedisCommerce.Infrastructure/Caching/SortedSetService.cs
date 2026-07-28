using RedisCommerce.Application.Interfaces;
using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Caching;

public class SortedSetService : RedisServiceBase, ISortedSetService
{
    public SortedSetService(IConnectionMultiplexer connectionMultiplexer)
        : base(connectionMultiplexer)
    {
    }

    public async Task<bool> AddAsync(string key, string member, double score) =>
        await Database.SortedSetAddAsync(key, member, score);

    public async Task<double> IncrementAsync(string key, string member, double increment) =>
        await Database.SortedSetIncrementAsync(key, member, increment);

    public async Task<IReadOnlyList<ScoredMember>> RangeDescendingAsync(string key, long start, long stop)
    {
        var entries = await Database.SortedSetRangeByRankWithScoresAsync(key, start, stop, Order.Descending);
        return entries.Select(e => new ScoredMember(e.Element.ToString(), e.Score)).ToList();
    }

    public async Task<double?> ScoreAsync(string key, string member) =>
        await Database.SortedSetScoreAsync(key, member);
}
