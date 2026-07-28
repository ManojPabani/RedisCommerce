using Microsoft.Extensions.Logging;
using RedisCommerce.Application.DTOs;
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
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ICartService cartService,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IOrderQueueRepository orderQueueRepository,
        IActivityTrackingService activityTracking,
        ILogger<OrderService> logger)
    {
        _cartService = cartService;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _orderQueueRepository = orderQueueRepository;
        _activityTracking = activityTracking;
        _logger = logger;
    }

    public async Task<OrderResponse> CheckoutAsync(CheckoutRequest request)
    {
        var cart = await _cartService.GetCartAsync(request.UserId);
        if (cart.Items.Count == 0)
        {
            throw new EmptyCartException(request.UserId);
        }

        var orderItems = new List<OrderItem>();
        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId)
                ?? throw new ProductNotFoundException(cartItem.ProductId);

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
        await _cartService.ClearCartAsync(request.UserId);

        await _orderQueueRepository.EnqueueAsync(created.Id);
        await _activityTracking.TrackActivityAsync(request.UserId, ActivityType.Checkout);
        _logger.LogInformation("Order Added To Queue: {OrderId}", created.Id);

        return created.ToResponse();
    }
}
