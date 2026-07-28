using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Services;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class VisitorAnalyticsServiceTests
{
    private readonly Mock<IVisitorAnalyticsRepository> _repository = new();
    private readonly VisitorAnalyticsService _sut;

    public VisitorAnalyticsServiceTests()
    {
        _sut = new VisitorAnalyticsService(_repository.Object, Mock.Of<ILogger<VisitorAnalyticsService>>());
    }

    [Fact]
    public async Task RecordVisitAsync_RecordsVisitForToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await _sut.RecordVisitAsync("visitor-123");

        _repository.Verify(r => r.RecordVisitAsync("visitor-123", today), Times.Once);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ReturnsAllFourCounts()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _repository.Setup(r => r.GetDailyCountAsync(today)).ReturnsAsync(10);
        _repository.Setup(r => r.GetWeeklyCountAsync(today)).ReturnsAsync(50);
        _repository.Setup(r => r.GetMonthlyCountAsync(today)).ReturnsAsync(200);
        _repository
            .Setup(r => r.GetMergedCountAsync(It.Is<IReadOnlyList<DateOnly>>(d => d.Count == 7)))
            .ReturnsAsync(60);

        var result = await _sut.GetDashboardSummaryAsync();

        Assert.Equal(10, result.DailyVisitors);
        Assert.Equal(50, result.WeeklyVisitors);
        Assert.Equal(200, result.MonthlyVisitors);
        Assert.Equal(60, result.MergedLast7DaysVisitors);
    }
}
