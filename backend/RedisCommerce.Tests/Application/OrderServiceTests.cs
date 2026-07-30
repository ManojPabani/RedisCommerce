using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Events;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Services;
using RedisCommerce.Domain.Entities;
using RedisCommerce.Domain.Exceptions;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class OrderServiceTests
{
    private readonly Mock<ICartService> _cartService = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IOrderQueueRepository> _orderQueueRepository = new();
    private readonly Mock<IActivityTrackingService> _activityTracking = new();
    private readonly Mock<IRedisPublisher> _publisher = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(
            _cartService.Object,
            _productRepository.Object,
            _orderRepository.Object,
            _orderQueueRepository.Object,
            _activityTracking.Object,
            _publisher.Object,
            Mock.Of<ILogger<OrderService>>());

        _orderRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order order) =>
            {
                order.Id = 123;
                return order;
            });
    }

    private static Product CreateProduct(int id, decimal price) => new()
    {
        Id = id,
        Name = $"Product {id}",
        Description = "Description",
        Price = price,
        StockQuantity = 100,
        CreatedDate = DateTime.UtcNow,
        UpdatedDate = DateTime.UtcNow,
    };

    [Fact]
    public async Task CheckoutAsync_EmptyCart_ThrowsEmptyCartException()
    {
        _cartService.Setup(c => c.GetCartAsync(1001)).ReturnsAsync(new CartResponse(1001, []));

        await Assert.ThrowsAsync<EmptyCartException>(() => _sut.CheckoutAsync(new CheckoutRequest(1001)));

        _orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_ValidCart_CreatesOrderWithTotalFromCurrentProductPrices()
    {
        _cartService.Setup(c => c.GetCartAsync(1001)).ReturnsAsync(new CartResponse(1001, [
            new CartItemResponse(10, 2),
            new CartItemResponse(50, 1),
        ]));
        _productRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new Dictionary<int, Product>
            {
                [10] = CreateProduct(10, 20m),
                [50] = CreateProduct(50, 15m),
            });

        var result = await _sut.CheckoutAsync(new CheckoutRequest(1001));

        Assert.Equal(55m, result.TotalAmount);
        Assert.Equal(123, result.Id);
        Assert.Equal("Pending", result.Status);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Orders, It.Is<OrderCreatedEvent>(e => e.Payload.OrderId == 123 && e.Payload.UserId == 1001)), Times.Once);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Cart, It.Is<CartCheckedOutEvent>(e => e.Payload.OrderId == 123)), Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_ValidCart_ClearsCartAndEnqueuesOrderId()
    {
        _cartService.Setup(c => c.GetCartAsync(1001)).ReturnsAsync(new CartResponse(1001, [
            new CartItemResponse(10, 1),
        ]));
        _productRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new Dictionary<int, Product>
            {
                [10] = CreateProduct(10, 20m),
            });

        await _sut.CheckoutAsync(new CheckoutRequest(1001));

        _cartService.Verify(c => c.ClearCartAsync(1001), Times.Once);
        _orderQueueRepository.Verify(q => q.EnqueueAsync(123), Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_ValidCart_EnqueuesBeforeClearingCart()
    {
        _cartService.Setup(c => c.GetCartAsync(1001)).ReturnsAsync(new CartResponse(1001, [
            new CartItemResponse(10, 1),
        ]));
        _productRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new Dictionary<int, Product>
            {
                [10] = CreateProduct(10, 20m),
            });

        var sequence = new MockSequence();
        _orderQueueRepository.InSequence(sequence).Setup(q => q.EnqueueAsync(123)).Returns(Task.CompletedTask);
        _cartService.InSequence(sequence).Setup(c => c.ClearCartAsync(1001)).Returns(Task.CompletedTask);

        await _sut.CheckoutAsync(new CheckoutRequest(1001));

        _orderQueueRepository.Verify(q => q.EnqueueAsync(123), Times.Once);
        _cartService.Verify(c => c.ClearCartAsync(1001), Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_CartReferencesDeletedProduct_ThrowsProductNotFoundException()
    {
        _cartService.Setup(c => c.GetCartAsync(1001)).ReturnsAsync(new CartResponse(1001, [
            new CartItemResponse(999, 1),
        ]));
        _productRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new Dictionary<int, Product>());

        await Assert.ThrowsAsync<ProductNotFoundException>(() => _sut.CheckoutAsync(new CheckoutRequest(1001)));

        _orderQueueRepository.Verify(q => q.EnqueueAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CancelAsync_PendingOrder_SetsCancelledAndPublishesEvents()
    {
        var order = new Order { Id = 5, UserId = 1001, Status = OrderStatus.Pending, TotalAmount = 20m };
        _orderRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(order);

        var result = await _sut.CancelAsync(5);

        Assert.Equal("Cancelled", result.Status);
        _orderRepository.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Cancelled)), Times.Once);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Orders, It.Is<OrderStatusChangedEvent>(
            e => e.Payload.OrderId == 5 && e.Payload.PreviousStatus == OrderStatus.Pending && e.Payload.NewStatus == OrderStatus.Cancelled)), Times.Once);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Orders, It.Is<OrderCancelledEvent>(e => e.Payload.OrderId == 5)), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_OrderDoesNotExist_ThrowsOrderNotFoundException()
    {
        _orderRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<OrderNotFoundException>(() => _sut.CancelAsync(999));
    }

    [Fact]
    public async Task CancelAsync_AlreadyDelivered_ThrowsInvalidOrderStateException()
    {
        var order = new Order { Id = 5, UserId = 1001, Status = OrderStatus.Delivered, TotalAmount = 20m };
        _orderRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(order);

        await Assert.ThrowsAsync<InvalidOrderStateException>(() => _sut.CancelAsync(5));

        _orderRepository.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }
}
