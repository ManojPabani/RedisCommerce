namespace RedisCommerce.Application.DTOs;

public record SessionResponse(
    string SessionId,
    int UserId,
    DateTime LoginTime,
    DateTime LastActivity,
    string IpAddress,
    string Browser,
    string Device);
