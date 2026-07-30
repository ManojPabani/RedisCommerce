using RedisCommerce.Domain.Entities;

namespace RedisCommerce.Domain.Exceptions;

public class InvalidOrderStateException : Exception
{
    public InvalidOrderStateException(int orderId, OrderStatus currentStatus)
        : base($"Order '{orderId}' cannot be cancelled from its current status '{currentStatus}'.")
    {
    }
}
