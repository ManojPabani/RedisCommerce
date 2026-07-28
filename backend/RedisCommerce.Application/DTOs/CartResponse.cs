namespace RedisCommerce.Application.DTOs;

public record CartResponse(int UserId, IReadOnlyList<CartItemResponse> Items);
