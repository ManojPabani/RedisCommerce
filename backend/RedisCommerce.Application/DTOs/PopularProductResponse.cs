namespace RedisCommerce.Application.DTOs;

public record PopularProductResponse(int ProductId, string Name, decimal Price, double ViewCount);
