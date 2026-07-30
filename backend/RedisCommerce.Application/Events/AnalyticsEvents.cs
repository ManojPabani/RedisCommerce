namespace RedisCommerce.Application.Events;

public sealed record AnalyticsGeneratedPayload(DateOnly Date, long ActiveUserCount);

public sealed record AnalyticsGeneratedEvent : BaseDomainEvent<AnalyticsGeneratedPayload>
{
    public override string EventType => DomainEventTypes.AnalyticsGenerated;
}
