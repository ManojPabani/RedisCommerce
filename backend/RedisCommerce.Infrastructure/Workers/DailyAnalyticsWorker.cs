using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Workers;

/// <summary>
/// Periodically snapshots today's active-user count into the analytics:daily-active-counts Sorted
/// Set (member = yyyyMMdd, score = active count), which powers the "Most Active Day" dashboard
/// widget. Redis TTL is per-key, not per-member, so a rolling window here needs explicit trimming
/// rather than EXPIRE — this worker also prunes entries older than the configured analytics TTL.
/// </summary>
public class DailyAnalyticsWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyAnalyticsWorker> _logger;

    public DailyAnalyticsWorker(IServiceScopeFactory scopeFactory, ILogger<DailyAnalyticsWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await GenerateSnapshotAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Daily analytics tick failed; will retry on the next interval");
            }
        }
    }

    private async Task GenerateSnapshotAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var activityTracking = scope.ServiceProvider.GetRequiredService<IActivityTrackingService>();
        var visitorAnalytics = scope.ServiceProvider.GetRequiredService<IVisitorAnalyticsService>();
        var sortedSetService = scope.ServiceProvider.GetRequiredService<ISortedSetService>();
        var ttlPolicy = scope.ServiceProvider.GetRequiredService<ITTLPolicyProvider>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeCount = await activityTracking.GetTodayCountAsync();
        var visitorSummary = await visitorAnalytics.GetDashboardSummaryAsync();

        await sortedSetService.AddAsync(CacheKeys.DailyActiveCounts, today.ToString("yyyyMMdd"), activeCount);

        var retentionDays = ttlPolicy.GetTtl(RedisObjectType.Analytics)!.Value.Days;
        await PruneOldEntriesAsync(sortedSetService, today, retentionDays);

        _logger.LogInformation(
            "Analytics Generated: {Date} active users={ActiveCount}, daily visitors={VisitorCount}",
            today,
            activeCount,
            visitorSummary.DailyVisitors);
    }

    private static async Task PruneOldEntriesAsync(ISortedSetService sortedSetService, DateOnly today, int retentionDays)
    {
        var cutoff = today.AddDays(-retentionDays);
        var all = await sortedSetService.RangeDescendingAsync(CacheKeys.DailyActiveCounts, 0, -1);

        foreach (var entry in all)
        {
            if (DateOnly.TryParseExact(entry.Member, "yyyyMMdd", out var entryDate) && entryDate < cutoff)
            {
                await sortedSetService.RemoveAsync(CacheKeys.DailyActiveCounts, entry.Member);
            }
        }
    }
}
