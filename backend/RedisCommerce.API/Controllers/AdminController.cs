using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IExpirationNotificationService _expirationNotificationService;
    private readonly IActivityTrackingService _activityTrackingService;
    private readonly IVisitorAnalyticsService _visitorAnalyticsService;

    public AdminController(
        ISessionService sessionService,
        IExpirationNotificationService expirationNotificationService,
        IActivityTrackingService activityTrackingService,
        IVisitorAnalyticsService visitorAnalyticsService)
    {
        _sessionService = sessionService;
        _expirationNotificationService = expirationNotificationService;
        _activityTrackingService = activityTrackingService;
        _visitorAnalyticsService = visitorAnalyticsService;
    }

    [HttpGet("sessions")]
    [ProducesResponseType(typeof(AdminSessionsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminSessionsResponse>> GetSessions()
    {
        var activeSessions = await _sessionService.GetActiveSessionsAsync();
        var recentExpirations = await _expirationNotificationService.GetRecentEventsAsync(20);

        return Ok(new AdminSessionsResponse(activeSessions.Count, activeSessions, recentExpirations));
    }

    [HttpGet("activity/today")]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    public async Task<ActionResult<long>> GetTodayActivity()
    {
        return Ok(await _activityTrackingService.GetTodayCountAsync());
    }

    [HttpGet("activity/count")]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    public async Task<ActionResult<long>> GetActivityCount([FromQuery] int days = 1)
    {
        return Ok(await _activityTrackingService.GetLastNDaysUniqueCountAsync(days));
    }

    [HttpGet("activity/summary")]
    [ProducesResponseType(typeof(ActivitySummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ActivitySummaryResponse>> GetActivitySummary()
    {
        var today = await _activityTrackingService.GetTodayCountAsync();
        var yesterday = await _activityTrackingService.GetCountForDateAsync(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
        var last7Days = await _activityTrackingService.GetLastNDaysUniqueCountAsync(7);
        var last30Days = await _activityTrackingService.GetLastNDaysUniqueCountAsync(30);

        return Ok(new ActivitySummaryResponse(today, yesterday, last7Days, last30Days));
    }

    [HttpGet("activity/most-active-day")]
    [ProducesResponseType(typeof(MostActiveDayResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MostActiveDayResponse>> GetMostActiveDay()
    {
        return Ok(await _activityTrackingService.GetMostActiveDayAsync());
    }

    [HttpGet("activity/{userId:int}")]
    [ProducesResponseType(typeof(UserActivityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserActivityResponse>> GetUserActivity(int userId)
    {
        var activeToday = await _activityTrackingService.IsUserActiveTodayAsync(userId);
        return Ok(new UserActivityResponse(userId, activeToday));
    }

    [HttpGet("visitors")]
    [ProducesResponseType(typeof(VisitorAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<VisitorAnalyticsResponse>> GetVisitors()
    {
        return Ok(await _visitorAnalyticsService.GetDashboardSummaryAsync());
    }
}
