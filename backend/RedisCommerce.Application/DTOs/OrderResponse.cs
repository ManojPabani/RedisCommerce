namespace RedisCommerce.Application.DTOs;

public record OrderResponse(
    int Id,
    int UserId,
    string Status,
    decimal TotalAmount,
    IReadOnlyList<OrderItemResponse> Items,
    DateTime CreatedDate);
