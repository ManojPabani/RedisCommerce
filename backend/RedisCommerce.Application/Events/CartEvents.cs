namespace RedisCommerce.Application.Events;

public sealed record CartUpdatedPayload(int UserId, int ItemCount);

public sealed record CartUpdatedEvent : BaseDomainEvent<CartUpdatedPayload>
{
    public override string EventType => DomainEventTypes.CartUpdated;
}

public sealed record CartCheckedOutPayload(int UserId, int OrderId, decimal TotalAmount);

public sealed record CartCheckedOutEvent : BaseDomainEvent<CartCheckedOutPayload>
{
    public override string EventType => DomainEventTypes.CartCheckedOut;
}
