namespace RedisCommerce.Application.DTOs;

public record AdminSessionsResponse(
    int ActiveSessionCount,
    IReadOnlyList<SessionResponse> ActiveSessions,
    IReadOnlyList<ExpirationEventResponse> RecentExpirations);
