using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Repositories;

public class RedisProductPopularityRepository : IProductPopularityRepository
{
    private readonly ISortedSetService _sortedSetService;

    public RedisProductPopularityRepository(ISortedSetService sortedSetService)
    {
        _sortedSetService = sortedSetService;
    }

    public async Task<double> IncrementAsync(int productId) =>
        await _sortedSetService.IncrementAsync(CacheKeys.PopularProducts, productId.ToString(), 1);

    public async Task<IReadOnlyList<ScoredMember>> GetTopAsync(int count) =>
        await _sortedSetService.RangeDescendingAsync(CacheKeys.PopularProducts, 0, count - 1);
}
