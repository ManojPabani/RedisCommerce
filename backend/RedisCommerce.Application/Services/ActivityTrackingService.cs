using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Application.Services;

public class ActivityTrackingService : IActivityTrackingService
{
    private readonly IActivityTrackingRepository _repository;
    private readonly ISortedSetService _sortedSetService;
    private readonly ILogger<ActivityTrackingService> _logger;

    public ActivityTrackingService(
        IActivityTrackingRepository repository,
        ISortedSetService sortedSetService,
        ILogger<ActivityTrackingService> logger)
    {
        _repository = repository;
        _sortedSetService = sortedSetService;
        _logger = logger;
    }

    public async Task TrackActivityAsync(int userId, ActivityType type)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await _repository.MarkActiveAsync(today, userId);
        _logger.LogInformation("Bitmap Updated: {Key} user {UserId} ({ActivityType})", CacheKeys.Activity(today), userId, type);
    }

    public async Task<long> GetTodayCountAsync() =>
        await _repository.GetActiveCountAsync(DateOnly.FromDateTime(DateTime.UtcNow));

    public async Task<long> GetCountForDateAsync(DateOnly date) =>
        await _repository.GetActiveCountAsync(date);

    public async Task<long> GetLastNDaysUniqueCountAsync(int days)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dates = Enumerable.Range(0, days).Select(offset => today.AddDays(-offset)).ToList();
        return await _repository.GetUniqueCountOverRangeAsync(dates);
    }

    public async Task<bool> IsUserActiveTodayAsync(int userId) =>
        await _repository.IsActiveAsync(DateOnly.FromDateTime(DateTime.UtcNow), userId);

    public async Task<MostActiveDayResponse> GetMostActiveDayAsync()
    {
        var topEntry = await _sortedSetService.RangeDescendingAsync(CacheKeys.DailyActiveCounts, 0, 0);
        var entry = topEntry.FirstOrDefault();

        return entry is null
            ? new MostActiveDayResponse(null, 0)
            : new MostActiveDayResponse(entry.Member, (long)entry.Score);
    }
}
