using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.DTOs;
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
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(
            _cartService.Object,
            _productRepository.Object,
            _orderRepository.Object,
            _orderQueueRepository.Object,
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
        _productRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(CreateProduct(10, 20m));
        _productRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(CreateProduct(50, 15m));

        var result = await _sut.CheckoutAsync(new CheckoutRequest(1001));

        Assert.Equal(55m, result.TotalAmount);
        Assert.Equal(123, result.Id);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task CheckoutAsync_ValidCart_ClearsCartAndEnqueuesOrderId()
    {
        _cartService.Setup(c => c.GetCartAsync(1001)).ReturnsAsync(new CartResponse(1001, [
            new CartItemResponse(10, 1),
        ]));
        _productRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(CreateProduct(10, 20m));

        await _sut.CheckoutAsync(new CheckoutRequest(1001));

        _cartService.Verify(c => c.ClearCartAsync(1001), Times.Once);
        _orderQueueRepository.Verify(q => q.EnqueueAsync(123), Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_CartReferencesDeletedProduct_ThrowsProductNotFoundException()
    {
        _cartService.Setup(c => c.GetCartAsync(1001)).ReturnsAsync(new CartResponse(1001, [
            new CartItemResponse(999, 1),
        ]));
        _productRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<ProductNotFoundException>(() => _sut.CheckoutAsync(new CheckoutRequest(1001)));

        _orderQueueRepository.Verify(q => q.EnqueueAsync(It.IsAny<int>()), Times.Never);
    }
}
