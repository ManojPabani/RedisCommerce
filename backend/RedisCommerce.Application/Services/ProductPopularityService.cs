using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Application.Services;

public class ProductPopularityService : IProductPopularityService
{
    private readonly IProductPopularityRepository _popularityRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductPopularityService> _logger;

    public ProductPopularityService(
        IProductPopularityRepository popularityRepository,
        IProductRepository productRepository,
        ILogger<ProductPopularityService> logger)
    {
        _popularityRepository = popularityRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task RecordViewAsync(int productId)
    {
        await _popularityRepository.IncrementAsync(productId);
        _logger.LogInformation("Popularity Increased: {Key} product {ProductId}", CacheKeys.PopularProducts, productId);
    }

    public async Task<IReadOnlyList<PopularProductResponse>> GetTopProductsAsync(int count)
    {
        var topScored = await _popularityRepository.GetTopAsync(count);

        var popularProducts = new List<PopularProductResponse>();
        foreach (var scored in topScored)
        {
            var product = await _productRepository.GetByIdAsync(int.Parse(scored.Member));
            if (product is not null)
            {
                popularProducts.Add(new PopularProductResponse(product.Id, product.Name, product.Price, scored.Score));
            }
        }

        return popularProducts;
    }
}
