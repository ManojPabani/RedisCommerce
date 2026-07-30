using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Events;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Mapping;
using RedisCommerce.Application.Services;
using RedisCommerce.Domain.Entities;
using RedisCommerce.Domain.Exceptions;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repository = new();
    private readonly Mock<IRedisCacheService> _cache = new();
    private readonly Mock<IProductPopularityService> _popularity = new();
    private readonly Mock<ITTLPolicyProvider> _ttlPolicy = new();
    private readonly Mock<IActivityTrackingService> _activityTracking = new();
    private readonly Mock<IVisitorAnalyticsService> _visitorAnalytics = new();
    private readonly Mock<IRedisPublisher> _publisher = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _ttlPolicy.Setup(t => t.GetTtl(RedisObjectType.Product)).Returns(TimeSpan.FromMinutes(30));
        _sut = new ProductService(_repository.Object, _cache.Object, _popularity.Object, _ttlPolicy.Object, _activityTracking.Object, _visitorAnalytics.Object, _publisher.Object, Mock.Of<ILogger<ProductService>>());
    }

    private static Product CreateProduct(int id = 1) => new()
    {
        Id = id,
        Name = "Test Product",
        Description = "A product used for testing.",
        Price = 9.99m,
        StockQuantity = 10,
        CreatedDate = DateTime.UtcNow,
        UpdatedDate = DateTime.UtcNow,
    };

    [Fact]
    public async Task GetByIdAsync_CacheHit_ReturnsCachedValueWithoutHittingRepository()
    {
        var product = CreateProduct();
        var cachedJson = JsonSerializer.Serialize(product.ToResponse());
        _cache.Setup(c => c.GetAsync(CacheKeys.Product(product.Id))).ReturnsAsync(cachedJson);

        var result = await _sut.GetByIdAsync(product.Id);

        Assert.Equal(product.Id, result.Id);
        _repository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
        _popularity.Verify(p => p.RecordViewAsync(product.Id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_CacheMiss_FetchesFromRepositoryAndPopulatesCacheWithThirtyMinuteTtl()
    {
        var product = CreateProduct();
        _cache.Setup(c => c.GetAsync(CacheKeys.Product(product.Id))).ReturnsAsync((string?)null);
        _repository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(product.Id);

        Assert.Equal(product.Id, result.Id);
        _repository.Verify(r => r.GetByIdAsync(product.Id), Times.Once);
        _cache.Verify(c => c.SetAsync(
            CacheKeys.Product(product.Id),
            It.IsAny<string>(),
            TimeSpan.FromMinutes(30)),
            Times.Once);
        _popularity.Verify(p => p.RecordViewAsync(product.Id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ProductDoesNotExist_ThrowsProductNotFoundException()
    {
        _cache.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync((string?)null);
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<ProductNotFoundException>(() => _sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task CreateAsync_PublishesProductCreatedEvent()
    {
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = 42; return p; });

        var result = await _sut.CreateAsync(new CreateProductRequest("New Product", "Description", 12.5m, 3));

        Assert.Equal(42, result.Id);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Products, It.Is<ProductCreatedEvent>(e => e.Payload.ProductId == 42)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingProduct_InvalidatesCacheKeyAndPublishesProductUpdated()
    {
        var product = CreateProduct();
        _repository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        var request = new UpdateProductRequest("Updated Name", "Updated Description", product.Price, product.StockQuantity);

        await _sut.UpdateAsync(product.Id, request);

        _repository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync(CacheKeys.Product(product.Id)), Times.Once);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Products, It.Is<ProductUpdatedEvent>(e => e.Payload.ProductId == product.Id)), Times.Once);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Inventory, It.IsAny<InventoryUpdatedEvent>()), Times.Never);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Products, It.IsAny<PriceChangedEvent>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_StockQuantityChanged_PublishesInventoryUpdated()
    {
        var product = CreateProduct();
        var productId = product.Id;
        var newStockQuantity = product.StockQuantity + 5;
        _repository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        var request = new UpdateProductRequest(product.Name, product.Description, product.Price, newStockQuantity);

        await _sut.UpdateAsync(product.Id, request);

        _publisher.Verify(p => p.PublishAsync(RedisChannels.Inventory, It.Is<InventoryUpdatedEvent>(
            e => e.Payload.ProductId == productId && e.Payload.NewStockQuantity == newStockQuantity)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_PriceChanged_PublishesPriceChanged()
    {
        var product = CreateProduct();
        _repository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        var newPrice = product.Price + 5m;
        var request = new UpdateProductRequest(product.Name, product.Description, newPrice, product.StockQuantity);

        await _sut.UpdateAsync(product.Id, request);

        _publisher.Verify(p => p.PublishAsync(RedisChannels.Products, It.Is<PriceChangedEvent>(
            e => e.Payload.ProductId == product.Id && e.Payload.NewPrice == newPrice)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ProductDoesNotExist_ThrowsProductNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);
        var request = new UpdateProductRequest("Name", "Description", 1m, 1);

        await Assert.ThrowsAsync<ProductNotFoundException>(() => _sut.UpdateAsync(999, request));
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_InvalidatesCacheKey()
    {
        var product = CreateProduct();
        _repository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

        await _sut.DeleteAsync(product.Id);

        _repository.Verify(r => r.DeleteAsync(product), Times.Once);
        _cache.Verify(c => c.RemoveAsync(CacheKeys.Product(product.Id)), Times.Once);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Products, It.Is<ProductDeletedEvent>(e => e.Payload.ProductId == product.Id)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ProductDoesNotExist_ThrowsProductNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<ProductNotFoundException>(() => _sut.DeleteAsync(999));
    }

    [Fact]
    public async Task SearchAsync_BlankQuery_ReturnsEmptyWithoutHittingRepositoryOrActivity()
    {
        var result = await _sut.SearchAsync("   ");

        Assert.Empty(result);
        _repository.Verify(r => r.SearchAsync(It.IsAny<string>()), Times.Never);
        _activityTracking.Verify(a => a.TrackActivityAsync(It.IsAny<int>(), It.IsAny<ActivityType>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WithQuery_ReturnsMappedProductsAndTracksSearchActivity()
    {
        var product = CreateProduct();
        _repository.Setup(r => r.SearchAsync("keyboard")).ReturnsAsync([product]);

        var result = (await _sut.SearchAsync("keyboard", viewerUserId: 1001)).ToList();

        Assert.Single(result);
        Assert.Equal(product.Id, result[0].Id);
        _activityTracking.Verify(a => a.TrackActivityAsync(1001, ActivityType.Search), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_MalformedCachePayload_FallsBackToRepository()
    {
        var product = CreateProduct();
        _cache.Setup(c => c.GetAsync(CacheKeys.Product(product.Id))).ReturnsAsync("not-valid-json");
        _repository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(product.Id);

        Assert.Equal(product.Id, result.Id);
        _repository.Verify(r => r.GetByIdAsync(product.Id), Times.Once);
        _cache.Verify(c => c.SetAsync(
            CacheKeys.Product(product.Id),
            It.IsAny<string>(),
            TimeSpan.FromMinutes(30)),
            Times.Once);
    }
}
