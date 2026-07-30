namespace RedisCommerce.Application.Events;

public sealed record UserLoggedInPayload(int UserId, string SessionId);

public sealed record UserLoggedInEvent : BaseDomainEvent<UserLoggedInPayload>
{
    public override string EventType => DomainEventTypes.UserLoggedIn;
}

public sealed record UserLoggedOutPayload(int UserId, string SessionId);

public sealed record UserLoggedOutEvent : BaseDomainEvent<UserLoggedOutPayload>
{
    public override string EventType => DomainEventTypes.UserLoggedOut;
}

public sealed record FavoriteAddedPayload(int UserId, int ProductId);

public sealed record FavoriteAddedEvent : BaseDomainEvent<FavoriteAddedPayload>
{
    public override string EventType => DomainEventTypes.FavoriteAdded;
}

public sealed record FavoriteRemovedPayload(int UserId, int ProductId);

public sealed record FavoriteRemovedEvent : BaseDomainEvent<FavoriteRemovedPayload>
{
    public override string EventType => DomainEventTypes.FavoriteRemoved;
}
