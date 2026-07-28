using System.Globalization;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Repositories;

public class RedisVisitorAnalyticsRepository : IVisitorAnalyticsRepository
{
    private readonly IHyperLogLogService _hyperLogLogService;

    public RedisVisitorAnalyticsRepository(IHyperLogLogService hyperLogLogService)
    {
        _hyperLogLogService = hyperLogLogService;
    }

    public async Task RecordVisitAsync(string visitorId, DateOnly date)
    {
        var dailyKey = CacheKeys.VisitorsDaily(date);
        var weeklyKey = WeeklyKey(date);
        var monthlyKey = CacheKeys.VisitorsMonthly(date.Year, date.Month);

        await _hyperLogLogService.AddAsync(dailyKey, visitorId);
        await _hyperLogLogService.AddAsync(weeklyKey, visitorId);
        await _hyperLogLogService.AddAsync(monthlyKey, visitorId);
    }

    public async Task<long> GetDailyCountAsync(DateOnly date) =>
        await _hyperLogLogService.CountAsync(CacheKeys.VisitorsDaily(date));

    public async Task<long> GetWeeklyCountAsync(DateOnly date) =>
        await _hyperLogLogService.CountAsync(WeeklyKey(date));

    public async Task<long> GetMonthlyCountAsync(DateOnly date) =>
        await _hyperLogLogService.CountAsync(CacheKeys.VisitorsMonthly(date.Year, date.Month));

    public async Task<long> GetMergedCountAsync(IReadOnlyList<DateOnly> dailyDates)
    {
        if (dailyDates.Count == 0)
        {
            return 0;
        }

        var keys = dailyDates.Select(d => CacheKeys.VisitorsDaily(d)).ToArray();
        return await _hyperLogLogService.CountAsync(keys);
    }

    private static string WeeklyKey(DateOnly date)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        var isoYear = ISOWeek.GetYear(dateTime);
        var isoWeek = ISOWeek.GetWeekOfYear(dateTime);
        return CacheKeys.VisitorsWeekly(isoYear, isoWeek);
    }
}
