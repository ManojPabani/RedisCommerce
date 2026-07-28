using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Interfaces;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(int userId);
    Task<CartResponse> AddItemAsync(int userId, AddCartItemRequest request);
    Task<CartResponse> UpdateItemAsync(int userId, int productId, UpdateCartItemRequest request);
    Task<CartResponse> RemoveItemAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
}
