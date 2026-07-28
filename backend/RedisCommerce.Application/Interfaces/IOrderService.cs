using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CheckoutAsync(CheckoutRequest request);
}
