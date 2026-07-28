using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Services;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class ExpirationNotificationServiceTests
{
    private readonly Mock<IListService> _listService = new();
    private readonly Mock<ISessionRepository> _sessionRepository = new();
    private readonly ExpirationNotificationService _sut;

    public ExpirationNotificationServiceTests()
    {
        _sut = new ExpirationNotificationService(_listService.Object, _sessionRepository.Object, Mock.Of<ILogger<ExpirationNotificationService>>());
    }

    [Fact]
    public async Task HandleExpirationAsync_SessionKey_RemovesFromActiveSetAndRecordsEvent()
    {
        await _sut.HandleExpirationAsync("session:abc123");

        _sessionRepository.Verify(r => r.RemoveFromActiveSetAsync("abc123"), Times.Once);
        _listService.Verify(l => l.LeftPushAsync(CacheKeys.ExpirationEvents, It.IsAny<string>()), Times.Once);
        _listService.Verify(l => l.TrimAsync(CacheKeys.ExpirationEvents, 0, 99), Times.Once);
    }

    [Fact]
    public async Task HandleExpirationAsync_CartKey_RecordsEventWithoutTouchingSessionSet()
    {
        await _sut.HandleExpirationAsync("cart:1001");

        _sessionRepository.Verify(r => r.RemoveFromActiveSetAsync(It.IsAny<string>()), Times.Never);
        _listService.Verify(l => l.LeftPushAsync(CacheKeys.ExpirationEvents, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task HandleExpirationAsync_SessionUserMappingKey_IsIgnored()
    {
        await _sut.HandleExpirationAsync("session:user:1001");

        _sessionRepository.Verify(r => r.RemoveFromActiveSetAsync(It.IsAny<string>()), Times.Never);
        _listService.Verify(l => l.LeftPushAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleExpirationAsync_UnrelatedKey_IsIgnored()
    {
        await _sut.HandleExpirationAsync("product:1");

        _listService.Verify(l => l.LeftPushAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetRecentEventsAsync_DeserializesStoredEvents()
    {
        var event1 = new ExpirationEventResponse("session:abc", "Session Expired", DateTime.UtcNow);
        _listService
            .Setup(l => l.RangeAsync(CacheKeys.ExpirationEvents, 0, 9))
            .ReturnsAsync([JsonSerializer.Serialize(event1)]);

        var result = await _sut.GetRecentEventsAsync(10);

        Assert.Single(result);
        Assert.Equal("session:abc", result[0].Key);
        Assert.Equal("Session Expired", result[0].EventType);
    }

    [Fact]
    public async Task GetRecentEventsAsync_SkipsMalformedPayloads()
    {
        var event1 = new ExpirationEventResponse("session:abc", "Session Expired", DateTime.UtcNow);
        _listService
            .Setup(l => l.RangeAsync(CacheKeys.ExpirationEvents, 0, 9))
            .ReturnsAsync([JsonSerializer.Serialize(event1), "not-json"]);

        var result = await _sut.GetRecentEventsAsync(10);

        Assert.Single(result);
        Assert.Equal("session:abc", result[0].Key);
    }
}
