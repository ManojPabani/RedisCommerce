using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Services;
using RedisCommerce.Domain.Entities;
using RedisCommerce.Domain.Exceptions;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class FavoriteServiceTests
{
    private readonly Mock<IFavoriteRepository> _favoriteRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly FavoriteService _sut;

    public FavoriteServiceTests()
    {
        _sut = new FavoriteService(_favoriteRepository.Object, _productRepository.Object, Mock.Of<ILogger<FavoriteService>>());
    }

    private static Product CreateProduct(int id) => new()
    {
        Id = id,
        Name = $"Product {id}",
        Description = "Description",
        Price = 9.99m,
        StockQuantity = 100,
        CreatedDate = DateTime.UtcNow,
        UpdatedDate = DateTime.UtcNow,
    };

    [Fact]
    public async Task GetFavoritesAsync_ReturnsEnrichedProductDetails()
    {
        _favoriteRepository.Setup(r => r.GetAllAsync(1001)).ReturnsAsync([10, 50]);
        _productRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(CreateProduct(10));
        _productRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(CreateProduct(50));

        var result = await _sut.GetFavoritesAsync(1001);

        Assert.Equal(1001, result.UserId);
        Assert.Equal(2, result.Products.Count);
        Assert.Contains(result.Products, p => p.ProductId == 10);
    }

    [Fact]
    public async Task AddFavoriteAsync_NewFavorite_AddsToRepository()
    {
        _productRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(CreateProduct(10));
        _favoriteRepository.Setup(r => r.AddAsync(1001, 10)).ReturnsAsync(true);

        await _sut.AddFavoriteAsync(1001, 10);

        _favoriteRepository.Verify(r => r.AddAsync(1001, 10), Times.Once);
    }

    [Fact]
    public async Task AddFavoriteAsync_ProductDoesNotExist_ThrowsProductNotFoundException()
    {
        _productRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<ProductNotFoundException>(() => _sut.AddFavoriteAsync(1001, 999));

        _favoriteRepository.Verify(r => r.AddAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddFavoriteAsync_AlreadyFavorited_DoesNotThrowAndStaysIdempotent()
    {
        _productRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(CreateProduct(10));
        _favoriteRepository.Setup(r => r.AddAsync(1001, 10)).ReturnsAsync(false);

        await _sut.AddFavoriteAsync(1001, 10);

        _favoriteRepository.Verify(r => r.AddAsync(1001, 10), Times.Once);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_CallsRepositoryRemove()
    {
        _favoriteRepository.Setup(r => r.RemoveAsync(1001, 10)).ReturnsAsync(true);

        await _sut.RemoveFavoriteAsync(1001, 10);

        _favoriteRepository.Verify(r => r.RemoveAsync(1001, 10), Times.Once);
    }
}
