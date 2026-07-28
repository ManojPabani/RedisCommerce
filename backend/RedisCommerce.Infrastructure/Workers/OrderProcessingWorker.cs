using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Domain.Entities;

namespace RedisCommerce.Infrastructure.Workers;

public class OrderProcessingWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderProcessingWorker> _logger;

    public OrderProcessingWorker(IServiceScopeFactory scopeFactory, ILogger<OrderProcessingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessNextOrderAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Order processing tick failed; will retry on the next interval");
            }
        }
    }

    private async Task ProcessNextOrderAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var queueRepository = scope.ServiceProvider.GetRequiredService<IOrderQueueRepository>();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var orderId = await queueRepository.DequeueAsync();
        if (orderId is null)
        {
            return;
        }

        var order = await orderRepository.GetByIdAsync(orderId.Value);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} was dequeued but no longer exists", orderId.Value);
            return;
        }

        order.Status = OrderStatus.Processing;
        order.UpdatedDate = DateTime.UtcNow;
        await orderRepository.UpdateAsync(order);

        order.Status = OrderStatus.Completed;
        order.UpdatedDate = DateTime.UtcNow;
        await orderRepository.UpdateAsync(order);

        _logger.LogInformation("Order Processed: {OrderId}", order.Id);
    }
}
