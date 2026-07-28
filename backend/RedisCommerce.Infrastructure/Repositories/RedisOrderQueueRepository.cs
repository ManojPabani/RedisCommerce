using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Repositories;

public class RedisOrderQueueRepository : IOrderQueueRepository
{
    private readonly IListService _listService;

    public RedisOrderQueueRepository(IListService listService)
    {
        _listService = listService;
    }

    public async Task EnqueueAsync(int orderId)
    {
        await _listService.LeftPushAsync(CacheKeys.OrderProcessingQueue, orderId.ToString());
    }

    public async Task<int?> DequeueAsync()
    {
        var value = await _listService.RightPopAsync(CacheKeys.OrderProcessingQueue);
        if (value is null)
        {
            return null;
        }

        return int.TryParse(value, out var orderId) ? orderId : null;
    }

    public async Task<long> QueueLengthAsync() =>
        await _listService.LengthAsync(CacheKeys.OrderProcessingQueue);
}
