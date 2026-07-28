namespace RedisCommerce.Application.Interfaces;

public interface ICartRepository
{
    Task<IDictionary<int, int>> GetCartAsync(int userId);
    Task<int?> GetItemQuantityAsync(int userId, int productId);
    Task SetItemQuantityAsync(int userId, int productId, int quantity);
    Task RemoveItemAsync(int userId, int productId);
    Task ClearAsync(int userId);
    Task<long> ItemCountAsync(int userId);
}
