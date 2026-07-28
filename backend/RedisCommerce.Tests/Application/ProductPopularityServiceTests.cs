using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Services;
using RedisCommerce.Domain.Entities;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class ProductPopularityServiceTests
{
    private readonly Mock<IProductPopularityRepository> _popularityRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly ProductPopularityService _sut;

    public ProductPopularityServiceTests()
    {
        _sut = new ProductPopularityService(_popularityRepository.Object, _productRepository.Object, Mock.Of<ILogger<ProductPopularityService>>());
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
    public async Task RecordViewAsync_IncrementsScoreByOne()
    {
        await _sut.RecordViewAsync(10);

        _popularityRepository.Verify(r => r.IncrementAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetTopProductsAsync_ReturnsProductsOrderedByScoreDescending()
    {
        _popularityRepository.Setup(r => r.GetTopAsync(2)).ReturnsAsync([
            new ScoredMember("10", 5),
            new ScoredMember("50", 3),
        ]);
        _productRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(CreateProduct(10));
        _productRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(CreateProduct(50));

        var result = await _sut.GetTopProductsAsync(2);

        Assert.Equal(2, result.Count);
        Assert.Equal(10, result[0].ProductId);
        Assert.Equal(5, result[0].ViewCount);
        Assert.Equal(50, result[1].ProductId);
        Assert.Equal(3, result[1].ViewCount);
    }

    [Fact]
    public async Task GetTopProductsAsync_ProductWasDeleted_SkipsMissingProduct()
    {
        _popularityRepository.Setup(r => r.GetTopAsync(2)).ReturnsAsync([
            new ScoredMember("10", 5),
            new ScoredMember("999", 3),
        ]);
        _productRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(CreateProduct(10));
        _productRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _sut.GetTopProductsAsync(2);

        Assert.Single(result);
        Assert.Equal(10, result[0].ProductId);
    }
}
