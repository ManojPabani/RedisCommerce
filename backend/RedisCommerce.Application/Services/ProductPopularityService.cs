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

        var orderedIds = new List<int>();
        var scoresById = new Dictionary<int, double>();

        foreach (var scored in topScored)
        {
            if (!int.TryParse(scored.Member, out var productId))
            {
                _logger.LogWarning("Skipping non-numeric popularity member {Member}", scored.Member);
                continue;
            }

            orderedIds.Add(productId);
            scoresById[productId] = scored.Score;
        }

        var products = await _productRepository.GetByIdsAsync(orderedIds);

        return orderedIds
            .Where(products.ContainsKey)
            .Select(id =>
            {
                var product = products[id];
                return new PopularProductResponse(product.Id, product.Name, product.Price, scoresById[id]);
            })
            .ToList();
    }
}
