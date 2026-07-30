namespace RedisCommerce.Domain.Entities;

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    PaymentReceived,
    Packing,
    Shipping,
    Delivered,
    Cancelled,
}
