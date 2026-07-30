using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Events;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Domain.Entities;

namespace RedisCommerce.Infrastructure.Workers;

/// <summary>
/// Polls the order queue every 5 seconds and, for every order it dequeues, simulates a realistic
/// staged fulfilment pipeline (Pending -> PaymentReceived -> Processing -> Packing -> Shipping ->
/// Delivered), publishing an OrderStatusChanged event to events:orders at each transition. Each
/// order's pipeline runs as an independent, untracked Task so several orders can progress
/// concurrently without blocking the next poll tick.
/// </summary>
public class OrderProcessingWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan StageDelay = TimeSpan.FromSeconds(2);

    private static readonly OrderStatus[] Pipeline =
    [
        OrderStatus.PaymentReceived,
        OrderStatus.Processing,
        OrderStatus.Packing,
        OrderStatus.Shipping,
        OrderStatus.Delivered,
    ];

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
                await DequeueAndStartPipelinesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Order processing tick failed; will retry on the next interval");
            }
        }
    }

    private async Task DequeueAndStartPipelinesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var queueRepository = scope.ServiceProvider.GetRequiredService<IOrderQueueRepository>();

        while (true)
        {
            var orderId = await queueRepository.DequeueAsync();
            if (orderId is null)
            {
                return;
            }

            _ = ProcessOrderPipelineAsync(orderId.Value, stoppingToken);
        }
    }

    private async Task ProcessOrderPipelineAsync(int orderId, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IRedisPublisher>();

            var order = await orderRepository.GetByIdAsync(orderId);
            if (order is null)
            {
                _logger.LogWarning("Order {OrderId} was dequeued but no longer exists", orderId);
                return;
            }

            foreach (var status in Pipeline)
            {
                await Task.Delay(StageDelay, stoppingToken);

                var previousStatus = order.Status;
                order.Status = status;
                order.UpdatedDate = DateTime.UtcNow;
                await orderRepository.UpdateAsync(order);

                await publisher.PublishAsync(RedisChannels.Orders, new OrderStatusChangedEvent
                {
                    Payload = new OrderStatusChangedPayload(order.Id, order.UserId, previousStatus, status),
                });

                _logger.LogInformation("Order Status Changed: {OrderId} {Previous} -> {New}", order.Id, previousStatus, status);
            }

            _logger.LogInformation("Order Processed: {OrderId}", order.Id);
        }
        catch (OperationCanceledException)
        {
            // Host is shutting down; the order simply stays at its last-persisted status.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order pipeline failed for order {OrderId}", orderId);
        }
    }
}
