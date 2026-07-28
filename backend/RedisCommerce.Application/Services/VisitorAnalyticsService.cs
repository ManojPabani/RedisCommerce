using Microsoft.Extensions.Logging;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Application.Services;

public class VisitorAnalyticsService : IVisitorAnalyticsService
{
    private readonly IVisitorAnalyticsRepository _repository;
    private readonly ILogger<VisitorAnalyticsService> _logger;

    public VisitorAnalyticsService(IVisitorAnalyticsRepository repository, ILogger<VisitorAnalyticsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task RecordVisitAsync(string visitorId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await _repository.RecordVisitAsync(visitorId, today);

        _logger.LogInformation("Visitor Added: {VisitorId}", visitorId);
        _logger.LogInformation("HyperLogLog Updated: visitors:daily:{Date}", today);
    }

    public async Task<VisitorAnalyticsResponse> GetDashboardSummaryAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var last7Days = Enumerable.Range(0, 7).Select(offset => today.AddDays(-offset)).ToList();

        var daily = await _repository.GetDailyCountAsync(today);
        var weekly = await _repository.GetWeeklyCountAsync(today);
        var monthly = await _repository.GetMonthlyCountAsync(today);
        var merged = await _repository.GetMergedCountAsync(last7Days);

        return new VisitorAnalyticsResponse(daily, weekly, monthly, merged);
    }
}
