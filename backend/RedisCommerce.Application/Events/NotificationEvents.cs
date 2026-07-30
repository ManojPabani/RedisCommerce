using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Events;

/// <summary>
/// Raised by any subscriber that decides a user-facing notification is warranted. NotificationSubscriber is the
/// sole consumer — it persists the notification (via INotificationService) and pushes it over SignalR, so no
/// other component ever needs to know about the Hub directly.
/// </summary>
public sealed record NotificationCreatedPayload(
    int UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? RelatedEntityType = null,
    string? RelatedEntityId = null);

public sealed record NotificationCreatedEvent : BaseDomainEvent<NotificationCreatedPayload>
{
    public override string EventType => DomainEventTypes.NotificationCreated;
}
