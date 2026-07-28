namespace RedisCommerce.Application.Interfaces;

public interface IOrderQueueRepository
{
    Task EnqueueAsync(int orderId);
    Task<int?> DequeueAsync();
    Task<long> QueueLengthAsync();
}
