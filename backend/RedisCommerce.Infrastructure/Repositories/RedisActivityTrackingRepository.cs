using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Repositories;

public class RedisActivityTrackingRepository : IActivityTrackingRepository
{
    private readonly IBitmapService _bitmapService;

    public RedisActivityTrackingRepository(IBitmapService bitmapService)
    {
        _bitmapService = bitmapService;
    }

    public async Task MarkActiveAsync(DateOnly date, int userId)
    {
        await _bitmapService.SetBitAsync(CacheKeys.Activity(date), userId);
    }

    public async Task<long> GetActiveCountAsync(DateOnly date) =>
        await _bitmapService.CountAsync(CacheKeys.Activity(date));

    public async Task<bool> IsActiveAsync(DateOnly date, int userId) =>
        await _bitmapService.GetBitAsync(CacheKeys.Activity(date), userId);

    public async Task<long> GetUniqueCountOverRangeAsync(IReadOnlyList<DateOnly> dates)
    {
        if (dates.Count == 0)
        {
            return 0;
        }

        if (dates.Count == 1)
        {
            return await GetActiveCountAsync(dates[0]);
        }

        var scratchKey = $"activity:scratch:{Guid.NewGuid():N}";
        var sourceKeys = dates.Select(d => CacheKeys.Activity(d)).ToArray();

        await _bitmapService.BitOpAsync(BitwiseOperation.Or, scratchKey, sourceKeys);
        var count = await _bitmapService.CountAsync(scratchKey);
        await _bitmapService.DeleteAsync(scratchKey);

        return count;
    }
}
