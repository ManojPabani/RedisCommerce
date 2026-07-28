namespace RedisCommerce.Application.DTOs;

public record OrderItemResponse(int ProductId, string ProductName, decimal UnitPrice, int Quantity);
