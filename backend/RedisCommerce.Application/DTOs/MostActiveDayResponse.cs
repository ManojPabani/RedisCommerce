namespace RedisCommerce.Application.DTOs;

public record MostActiveDayResponse(string? Date, long ActiveUserCount);
