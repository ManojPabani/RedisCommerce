using RedisCommerce.Application.DTOs;
using RedisCommerce.Domain.Entities;

namespace RedisCommerce.Application.Mapping;

public static class ProductMapper
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.CreatedDate,
        product.UpdatedDate);
}
