namespace RedisCommerce.Application.DTOs;

public record FavoritesResponse(int UserId, IReadOnlyList<FavoriteProductResponse> Products);
