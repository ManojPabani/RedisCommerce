namespace RedisCommerce.Application.Events;

public sealed record SessionExpiredPayload(string SessionId, int? UserId);

public sealed record SessionExpiredEvent : BaseDomainEvent<SessionExpiredPayload>
{
    public override string EventType => DomainEventTypes.SessionExpired;
}
