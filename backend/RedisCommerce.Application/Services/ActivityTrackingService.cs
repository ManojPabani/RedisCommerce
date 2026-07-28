using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Application.Services;

public class ActivityTrackingService : IActivityTrackingService
{
    private readonly IActivityTrackingRepository _repository;
    private readonly ILogger<ActivityTrackingService> _logger;

    public ActivityTrackingService(IActivityTrackingRepository repository, ILogger<ActivityTrackingService> logger)
    {
        _repository = repository;
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
}
