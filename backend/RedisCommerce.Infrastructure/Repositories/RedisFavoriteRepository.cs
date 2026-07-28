using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Repositories;

public class RedisFavoriteRepository : IFavoriteRepository
{
    private readonly ISetService _setService;

    public RedisFavoriteRepository(ISetService setService)
    {
        _setService = setService;
    }

    public async Task<bool> AddAsync(int userId, int productId) =>
        await _setService.AddAsync(CacheKeys.Favorites(userId), productId.ToString());

    public async Task<bool> RemoveAsync(int userId, int productId) =>
        await _setService.RemoveAsync(CacheKeys.Favorites(userId), productId.ToString());

    public async Task<IReadOnlyList<int>> GetAllAsync(int userId)
    {
        var members = await _setService.MembersAsync(CacheKeys.Favorites(userId));
        return members
            .Select(member => int.TryParse(member, out var productId) ? productId : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
    }

    public async Task<bool> IsFavoriteAsync(int userId, int productId) =>
        await _setService.IsMemberAsync(CacheKeys.Favorites(userId), productId.ToString());

    public async Task<long> CountAsync(int userId) =>
        await _setService.CardinalityAsync(CacheKeys.Favorites(userId));
}
