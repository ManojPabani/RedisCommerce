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
        return value is null ? null : int.Parse(value);
    }

    public async Task<long> QueueLengthAsync() =>
        await _listService.LengthAsync(CacheKeys.OrderProcessingQueue);
}
