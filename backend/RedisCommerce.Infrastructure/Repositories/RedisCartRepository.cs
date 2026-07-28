using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Repositories;

public class RedisCartRepository : ICartRepository
{
    private readonly IHashService _hashService;
    private readonly ITTLPolicyProvider _ttlPolicy;

    public RedisCartRepository(IHashService hashService, ITTLPolicyProvider ttlPolicy)
    {
        _hashService = hashService;
        _ttlPolicy = ttlPolicy;
    }

    public async Task<IDictionary<int, int>> GetCartAsync(int userId)
    {
        var entries = await _hashService.HashGetAllAsync(CacheKeys.Cart(userId));
        var cart = new Dictionary<int, int>();

        foreach (var entry in entries)
        {
            if (int.TryParse(entry.Key, out var productId) && int.TryParse(entry.Value, out var quantity))
            {
                cart[productId] = quantity;
            }
        }

        return cart;
    }

    public async Task<int?> GetItemQuantityAsync(int userId, int productId)
    {
        var value = await _hashService.HashGetAsync(CacheKeys.Cart(userId), productId.ToString());
        return value is not null && int.TryParse(value, out var quantity) ? quantity : null;
    }

    public async Task SetItemQuantityAsync(int userId, int productId, int quantity)
    {
        var key = CacheKeys.Cart(userId);
        await _hashService.HashSetAsync(key, productId.ToString(), quantity.ToString());
        await _hashService.ExpireAsync(key, _ttlPolicy.GetTtl(RedisObjectType.Cart)!.Value);
    }

    public async Task RemoveItemAsync(int userId, int productId)
    {
        var key = CacheKeys.Cart(userId);
        await _hashService.HashDeleteAsync(key, productId.ToString());
        await _hashService.ExpireAsync(key, _ttlPolicy.GetTtl(RedisObjectType.Cart)!.Value);
    }

    public async Task ClearAsync(int userId)
    {
        await _hashService.DeleteAsync(CacheKeys.Cart(userId));
    }

    public async Task<long> ItemCountAsync(int userId) =>
        await _hashService.HashLengthAsync(CacheKeys.Cart(userId));
}
