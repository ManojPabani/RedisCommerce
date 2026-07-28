using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Domain.Exceptions;

namespace RedisCommerce.Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(
        IFavoriteRepository favoriteRepository,
        IProductRepository productRepository,
        ILogger<FavoriteService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<FavoritesResponse> GetFavoritesAsync(int userId)
    {
        var productIds = await _favoriteRepository.GetAllAsync(userId);

        var favoriteProducts = new List<FavoriteProductResponse>();
        foreach (var productId in productIds)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product is not null)
            {
                favoriteProducts.Add(new FavoriteProductResponse(product.Id, product.Name, product.Price));
            }
        }

        return new FavoritesResponse(userId, favoriteProducts);
    }

    public async Task AddFavoriteAsync(int userId, int productId)
    {
        _ = await _productRepository.GetByIdAsync(productId)
            ?? throw new ProductNotFoundException(productId);

        var added = await _favoriteRepository.AddAsync(userId, productId);
        if (added)
        {
            _logger.LogInformation("Favorite Added: {Key} product {ProductId}", CacheKeys.Favorites(userId), productId);
        }
    }

    public async Task RemoveFavoriteAsync(int userId, int productId)
    {
        var removed = await _favoriteRepository.RemoveAsync(userId, productId);
        if (removed)
        {
            _logger.LogInformation("Favorite Removed: {Key} product {ProductId}", CacheKeys.Favorites(userId), productId);
        }
    }
}
