using RedisCommerce.Domain.Entities;

namespace RedisCommerce.Application.Events;

public sealed record OrderCreatedPayload(int OrderId, int UserId, decimal TotalAmount, int ItemCount);

public sealed record OrderCreatedEvent : BaseDomainEvent<OrderCreatedPayload>
{
    public override string EventType => DomainEventTypes.OrderCreated;
}

public sealed record OrderStatusChangedPayload(int OrderId, int UserId, OrderStatus PreviousStatus, OrderStatus NewStatus);

public sealed record OrderStatusChangedEvent : BaseDomainEvent<OrderStatusChangedPayload>
{
    public override string EventType => DomainEventTypes.OrderStatusChanged;
}

public sealed record OrderCancelledPayload(int OrderId, int UserId);

public sealed record OrderCancelledEvent : BaseDomainEvent<OrderCancelledPayload>
{
    public override string EventType => DomainEventTypes.OrderCancelled;
}
