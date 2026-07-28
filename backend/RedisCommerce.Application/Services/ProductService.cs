using System.Text.Json;
using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Mapping;
using RedisCommerce.Domain.Entities;
using RedisCommerce.Domain.Exceptions;

namespace RedisCommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IRedisCacheService _cache;
    private readonly IProductPopularityService _popularity;
    private readonly ITTLPolicyProvider _ttlPolicy;
    private readonly IActivityTrackingService _activityTracking;
    private readonly IVisitorAnalyticsService _visitorAnalytics;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IRedisCacheService cache,
        IProductPopularityService popularity,
        ITTLPolicyProvider ttlPolicy,
        IActivityTrackingService activityTracking,
        IVisitorAnalyticsService visitorAnalytics,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _cache = cache;
        _popularity = popularity;
        _ttlPolicy = ttlPolicy;
        _activityTracking = activityTracking;
        _visitorAnalytics = visitorAnalytics;
        _logger = logger;
    }

    public async Task<ProductResponse> GetByIdAsync(int id, int? viewerUserId = null, string? visitorId = null)
    {
        var key = CacheKeys.Product(id);
        var cached = await _cache.GetAsync(key);

        ProductResponse? response = null;
        if (cached is not null)
        {
            try
            {
                response = JsonSerializer.Deserialize<ProductResponse>(cached);
            }
            catch (JsonException)
            {
                _logger.LogWarning("Malformed cache payload for {Key}; treating as miss", key);
            }

            if (response is not null)
            {
                _logger.LogInformation("CACHE HIT: {Key}", key);
                await _popularity.RecordViewAsync(id);
            }
        }

        if (response is null)
        {
            _logger.LogInformation("CACHE MISS: {Key}", key);

            var product = await _repository.GetByIdAsync(id)
                ?? throw new ProductNotFoundException(id);

            response = product.ToResponse();
            var ttl = _ttlPolicy.GetTtl(RedisObjectType.Product)!.Value;
            await _cache.SetAsync(key, JsonSerializer.Serialize(response), ttl);
            await _popularity.RecordViewAsync(id);
        }

        if (viewerUserId.HasValue)
        {
            await _activityTracking.TrackActivityAsync(viewerUserId.Value, ActivityType.ProductView);
        }

        if (!string.IsNullOrWhiteSpace(visitorId))
        {
            await _visitorAnalytics.RecordVisitAsync(visitorId);
        }

        return response;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        var products = await _repository.GetAllAsync();
        return products.Select(p => p.ToResponse());
    }

    public async Task<IEnumerable<ProductResponse>> SearchAsync(string query, int? viewerUserId = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var products = await _repository.SearchAsync(query);

        if (viewerUserId.HasValue)
        {
            await _activityTracking.TrackActivityAsync(viewerUserId.Value, ActivityType.Search);
        }

        return products.Select(p => p.ToResponse());
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var now = DateTime.UtcNow;
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CreatedDate = now,
            UpdatedDate = now,
        };

        var created = await _repository.AddAsync(product);
        return created.ToResponse();
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _repository.GetByIdAsync(id)
            ?? throw new ProductNotFoundException(id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(product);

        var key = CacheKeys.Product(id);
        await _cache.RemoveAsync(key);
        _logger.LogInformation("CACHE UPDATED: {Key}", key);

        return product.ToResponse();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id)
            ?? throw new ProductNotFoundException(id);

        await _repository.DeleteAsync(product);

        var key = CacheKeys.Product(id);
        await _cache.RemoveAsync(key);
        _logger.LogInformation("CACHE REMOVED: {Key}", key);
    }
}
