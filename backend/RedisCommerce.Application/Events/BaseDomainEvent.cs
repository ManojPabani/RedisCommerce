namespace RedisCommerce.Application.Events;

/// <summary>
/// Common envelope every domain event carries — metadata only, no business data.
/// Concrete events derive from <see cref="BaseDomainEvent{TPayload}"/> and supply the strongly typed payload.
/// </summary>
public abstract record BaseDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public abstract string EventType { get; }

    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    public string Source { get; init; } = "RedisCommerce.API";

    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public abstract record BaseDomainEvent<TPayload> : BaseDomainEvent
{
    public required TPayload Payload { get; init; }
}
