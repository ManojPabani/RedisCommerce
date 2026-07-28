namespace RedisCommerce.Application.DTOs;

public record ActivitySummaryResponse(long Today, long Yesterday, long Last7Days, long Last30Days);
