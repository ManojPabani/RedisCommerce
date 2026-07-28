using RedisCommerce.Application.DTOs;
using RedisCommerce.Domain.Entities;

namespace RedisCommerce.Application.Mapping;

public static class OrderMapper
{
    public static OrderResponse ToResponse(this Order order) => new(
        order.Id,
        order.UserId,
        order.Status.ToString(),
        order.TotalAmount,
        order.Items.Select(i => new OrderItemResponse(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(),
        order.CreatedDate);
}
