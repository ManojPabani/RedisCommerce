namespace RedisCommerce.Application.Interfaces;

public interface IVisitorAnalyticsRepository
{
    Task RecordVisitAsync(string visitorId, DateOnly date);
    Task<long> GetDailyCountAsync(DateOnly date);
    Task<long> GetWeeklyCountAsync(DateOnly date);
    Task<long> GetMonthlyCountAsync(DateOnly date);
    Task<long> GetMergedCountAsync(IReadOnlyList<DateOnly> dailyDates);
}
