using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Interfaces;

public interface IVisitorAnalyticsService
{
    Task RecordVisitAsync(string visitorId);
    Task<VisitorAnalyticsResponse> GetDashboardSummaryAsync();
}
