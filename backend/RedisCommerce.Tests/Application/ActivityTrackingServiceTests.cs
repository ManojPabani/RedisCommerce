using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Services;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class ActivityTrackingServiceTests
{
    private readonly Mock<IActivityTrackingRepository> _repository = new();
    private readonly ActivityTrackingService _sut;

    public ActivityTrackingServiceTests()
    {
        _sut = new ActivityTrackingService(_repository.Object, Mock.Of<ILogger<ActivityTrackingService>>());
    }

    [Fact]
    public async Task TrackActivityAsync_MarksTodaysBitForUser()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await _sut.TrackActivityAsync(1001, ActivityType.ProductView);

        _repository.Verify(r => r.MarkActiveAsync(today, 1001), Times.Once);
    }

    [Fact]
    public async Task GetTodayCountAsync_ReturnsTodaysActiveCount()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _repository.Setup(r => r.GetActiveCountAsync(today)).ReturnsAsync(42);

        var result = await _sut.GetTodayCountAsync();

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task GetLastNDaysUniqueCountAsync_PassesCorrectNumberOfDatesToRepository()
    {
        _repository
            .Setup(r => r.GetUniqueCountOverRangeAsync(It.Is<IReadOnlyList<DateOnly>>(d => d.Count == 7)))
            .ReturnsAsync(100);

        var result = await _sut.GetLastNDaysUniqueCountAsync(7);

        Assert.Equal(100, result);
    }

    [Fact]
    public async Task IsUserActiveTodayAsync_DelegatesToRepositoryForToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _repository.Setup(r => r.IsActiveAsync(today, 1001)).ReturnsAsync(true);

        var result = await _sut.IsUserActiveTodayAsync(1001);

        Assert.True(result);
    }
}
