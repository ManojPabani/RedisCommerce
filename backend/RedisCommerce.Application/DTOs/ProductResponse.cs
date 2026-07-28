namespace RedisCommerce.Application.DTOs;

public record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedDate,
    DateTime UpdatedDate);
