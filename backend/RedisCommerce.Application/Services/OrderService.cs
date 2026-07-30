using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Events;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Mapping;
using RedisCommerce.Domain.Entities;
using RedisCommerce.Domain.Exceptions;

namespace RedisCommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly ICartService _cartService;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderQueueRepository _orderQueueRepository;
    private readonly IActivityTrackingService _activityTracking;
    private readonly IRedisPublisher _publisher;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ICartService cartService,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IOrderQueueRepository orderQueueRepository,
        IActivityTrackingService activityTracking,
        IRedisPublisher publisher,
        ILogger<OrderService> logger)
    {
        _cartService = cartService;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _orderQueueRepository = orderQueueRepository;
        _activityTracking = activityTracking;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<OrderResponse> CheckoutAsync(CheckoutRequest request)
    {
        var cart = await _cartService.GetCartAsync(request.UserId);
        if (cart.Items.Count == 0)
        {
            throw new EmptyCartException(request.UserId);
        }

        var productIds = cart.Items.Select(item => item.ProductId);
        var products = await _productRepository.GetByIdsAsync(productIds);

        var orderItems = new List<OrderItem>();
        foreach (var cartItem in cart.Items)
        {
            if (!products.TryGetValue(cartItem.ProductId, out var product))
            {
                throw new ProductNotFoundException(cartItem.ProductId);
            }

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity,
            });
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            UserId = request.UserId,
            Status = OrderStatus.Pending,
            TotalAmount = orderItems.Sum(i => i.UnitPrice * i.Quantity),
            CreatedDate = now,
            UpdatedDate = now,
            Items = orderItems,
        };

        var created = await _orderRepository.AddAsync(order);
        await _orderQueueRepository.EnqueueAsync(created.Id);
        await _cartService.ClearCartAsync(request.UserId);
        await _activityTracking.TrackActivityAsync(request.UserId, ActivityType.Checkout);
        _logger.LogInformation("Order Added To Queue: {OrderId}", created.Id);

        await _publisher.PublishAsync(RedisChannels.Orders, new OrderCreatedEvent
        {
            Payload = new OrderCreatedPayload(created.Id, created.UserId, created.TotalAmount, created.Items.Count),
        });
        await _publisher.PublishAsync(RedisChannels.Cart, new CartCheckedOutEvent
        {
            Payload = new CartCheckedOutPayload(created.UserId, created.Id, created.TotalAmount),
        });

        return created.ToResponse();
    }

    public async Task<OrderResponse> CancelAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new OrderNotFoundException(orderId);

        if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            throw new InvalidOrderStateException(orderId, order.Status);
        }

        var previousStatus = order.Status;
        order.Status = OrderStatus.Cancelled;
        order.UpdatedDate = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Order Cancelled: {OrderId}", order.Id);

        await _publisher.PublishAsync(RedisChannels.Orders, new OrderStatusChangedEvent
        {
            Payload = new OrderStatusChangedPayload(order.Id, order.UserId, previousStatus, OrderStatus.Cancelled),
        });
        await _publisher.PublishAsync(RedisChannels.Orders, new OrderCancelledEvent
        {
            Payload = new OrderCancelledPayload(order.Id, order.UserId),
        });

        return order.ToResponse();
    }
}
