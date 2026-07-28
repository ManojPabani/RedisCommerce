using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Interfaces;

public interface IFavoriteService
{
    Task<FavoritesResponse> GetFavoritesAsync(int userId);
    Task AddFavoriteAsync(int userId, int productId);
    Task RemoveFavoriteAsync(int userId, int productId);
}
