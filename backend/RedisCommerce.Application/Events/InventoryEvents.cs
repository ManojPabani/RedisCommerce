namespace RedisCommerce.Application.Events;

public sealed record InventoryUpdatedPayload(int ProductId, int OldStockQuantity, int NewStockQuantity, bool InStock);

public sealed record InventoryUpdatedEvent : BaseDomainEvent<InventoryUpdatedPayload>
{
    public override string EventType => DomainEventTypes.InventoryUpdated;
}
