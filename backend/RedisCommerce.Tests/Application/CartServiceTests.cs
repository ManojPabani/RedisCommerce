using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Services;
using RedisCommerce.Domain.Entities;
using RedisCommerce.Domain.Exceptions;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly CartService _sut;

    public CartServiceTests()
    {
        _sut = new CartService(_cartRepository.Object, _productRepository.Object, Mock.Of<ILogger<CartService>>());
    }

    private static Product CreateProduct(int id = 10) => new()
    {
        Id = id,
        Name = "Test Product",
        Description = "Description",
        Price = 9.99m,
        StockQuantity = 100,
        CreatedDate = DateTime.UtcNow,
        UpdatedDate = DateTime.UtcNow,
    };

    [Fact]
    public async Task GetCartAsync_EmptyCart_ReturnsEmptyResponseWithoutThrowing()
    {
        _cartRepository.Setup(r => r.GetCartAsync(1001)).ReturnsAsync(new Dictionary<int, int>());

        var result = await _sut.GetCartAsync(1001);

        Assert.Equal(1001, result.UserId);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task AddItemAsync_NewProduct_SetsQuantityToRequestedAmount()
    {
        _productRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(CreateProduct());
        _cartRepository.Setup(r => r.GetItemQuantityAsync(1001, 10)).ReturnsAsync((int?)null);
        _cartRepository.Setup(r => r.GetCartAsync(1001)).ReturnsAsync(new Dictionary<int, int> { [10] = 2 });

        await _sut.AddItemAsync(1001, new AddCartItemRequest(10, 2));

        _cartRepository.Verify(r => r.SetItemQuantityAsync(1001, 10, 2), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ExistingProduct_IncreasesQuantity()
    {
        _productRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(CreateProduct());
        _cartRepository.Setup(r => r.GetItemQuantityAsync(1001, 10)).ReturnsAsync(3);
        _cartRepository.Setup(r => r.GetCartAsync(1001)).ReturnsAsync(new Dictionary<int, int> { [10] = 5 });

        await _sut.AddItemAsync(1001, new AddCartItemRequest(10, 2));

        _cartRepository.Verify(r => r.SetItemQuantityAsync(1001, 10, 5), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ProductDoesNotExist_ThrowsProductNotFoundException()
    {
        _productRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<ProductNotFoundException>(
            () => _sut.AddItemAsync(1001, new AddCartItemRequest(999, 1)));
    }

    [Fact]
    public async Task UpdateItemAsync_PositiveQuantity_SetsQuantity()
    {
        _cartRepository.Setup(r => r.GetCartAsync(1001)).ReturnsAsync(new Dictionary<int, int> { [10] = 4 });

        await _sut.UpdateItemAsync(1001, 10, new UpdateCartItemRequest(4));

        _cartRepository.Verify(r => r.SetItemQuantityAsync(1001, 10, 4), Times.Once);
        _cartRepository.Verify(r => r.RemoveItemAsync(1001, 10), Times.Never);
    }

    [Fact]
    public async Task UpdateItemAsync_ZeroQuantity_RemovesItemInsteadOfStoringZero()
    {
        _cartRepository.Setup(r => r.GetCartAsync(1001)).ReturnsAsync(new Dictionary<int, int>());

        await _sut.UpdateItemAsync(1001, 10, new UpdateCartItemRequest(0));

        _cartRepository.Verify(r => r.RemoveItemAsync(1001, 10), Times.Once);
        _cartRepository.Verify(r => r.SetItemQuantityAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_CallsRepositoryRemove()
    {
        _cartRepository.Setup(r => r.GetCartAsync(1001)).ReturnsAsync(new Dictionary<int, int>());

        await _sut.RemoveItemAsync(1001, 10);

        _cartRepository.Verify(r => r.RemoveItemAsync(1001, 10), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_CallsRepositoryClear()
    {
        await _sut.ClearCartAsync(1001);

        _cartRepository.Verify(r => r.ClearAsync(1001), Times.Once);
    }
}
