namespace RedisCommerce.Application.DTOs;

public record VisitorAnalyticsResponse(
    long DailyVisitors,
    long WeeklyVisitors,
    long MonthlyVisitors,
    long MergedLast7DaysVisitors);
