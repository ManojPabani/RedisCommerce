namespace RedisCommerce.Application.Events;

public sealed record ProductCreatedPayload(int ProductId, string Name, decimal Price, int StockQuantity);

public sealed record ProductCreatedEvent : BaseDomainEvent<ProductCreatedPayload>
{
    public override string EventType => DomainEventTypes.ProductCreated;
}

public sealed record ProductUpdatedPayload(int ProductId, string Name, decimal Price, int StockQuantity);

public sealed record ProductUpdatedEvent : BaseDomainEvent<ProductUpdatedPayload>
{
    public override string EventType => DomainEventTypes.ProductUpdated;
}

public sealed record ProductDeletedPayload(int ProductId, string Name);

public sealed record ProductDeletedEvent : BaseDomainEvent<ProductDeletedPayload>
{
    public override string EventType => DomainEventTypes.ProductDeleted;
}

public sealed record PriceChangedPayload(int ProductId, decimal OldPrice, decimal NewPrice);

public sealed record PriceChangedEvent : BaseDomainEvent<PriceChangedPayload>
{
    public override string EventType => DomainEventTypes.PriceChanged;
}
