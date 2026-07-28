using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Repositories;

public class RedisCartRepository : ICartRepository
{
    private static readonly TimeSpan CartExpiration = TimeSpan.FromHours(24);

    private readonly IHashService _hashService;

    public RedisCartRepository(IHashService hashService)
    {
        _hashService = hashService;
    }

    public async Task<IDictionary<int, int>> GetCartAsync(int userId)
    {
        var entries = await _hashService.HashGetAllAsync(CacheKeys.Cart(userId));
        return entries.ToDictionary(e => int.Parse(e.Key), e => int.Parse(e.Value));
    }

    public async Task<int?> GetItemQuantityAsync(int userId, int productId)
    {
        var value = await _hashService.HashGetAsync(CacheKeys.Cart(userId), productId.ToString());
        return value is null ? null : int.Parse(value);
    }

    public async Task SetItemQuantityAsync(int userId, int productId, int quantity)
    {
        var key = CacheKeys.Cart(userId);
        await _hashService.HashSetAsync(key, productId.ToString(), quantity.ToString());
        await _hashService.ExpireAsync(key, CartExpiration);
    }

    public async Task RemoveItemAsync(int userId, int productId)
    {
        var key = CacheKeys.Cart(userId);
        await _hashService.HashDeleteAsync(key, productId.ToString());
        await _hashService.ExpireAsync(key, CartExpiration);
    }

    public async Task ClearAsync(int userId)
    {
        await _hashService.DeleteAsync(CacheKeys.Cart(userId));
    }

    public async Task<long> ItemCountAsync(int userId) =>
        await _hashService.HashLengthAsync(CacheKeys.Cart(userId));
}
