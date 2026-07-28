namespace RedisCommerce.Application.DTOs;

public record ExpirationEventResponse(string Key, string EventType, DateTime OccurredAt);
