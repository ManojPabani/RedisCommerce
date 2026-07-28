namespace RedisCommerce.Application.Interfaces;

public interface IFavoriteRepository
{
    Task<bool> AddAsync(int userId, int productId);
    Task<bool> RemoveAsync(int userId, int productId);
    Task<IReadOnlyList<int>> GetAllAsync(int userId);
    Task<bool> IsFavoriteAsync(int userId, int productId);
    Task<long> CountAsync(int userId);
}
