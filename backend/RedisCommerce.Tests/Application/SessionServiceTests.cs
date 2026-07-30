using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Events;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Services;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _repository = new();
    private readonly Mock<ITTLPolicyProvider> _ttlPolicy = new();
    private readonly Mock<IActivityTrackingService> _activityTracking = new();
    private readonly Mock<IRedisPublisher> _publisher = new();
    private readonly SessionService _sut;

    public SessionServiceTests()
    {
        _ttlPolicy.Setup(t => t.GetTtl(RedisObjectType.Session)).Returns(TimeSpan.FromMinutes(30));
        _sut = new SessionService(_repository.Object, _ttlPolicy.Object, _activityTracking.Object, _publisher.Object, Mock.Of<ILogger<SessionService>>());
    }

    [Fact]
    public async Task LoginAsync_NoExistingSession_CreatesNewSession()
    {
        _repository.Setup(r => r.GetActiveSessionIdForUserAsync(1001)).ReturnsAsync((string?)null);

        var result = await _sut.LoginAsync(new LoginRequest(1001), "127.0.0.1", "Mozilla/5.0 Chrome/1.0");

        Assert.Equal(1001, result.UserId);
        Assert.False(string.IsNullOrWhiteSpace(result.SessionId));
        _repository.Verify(r => r.SaveAsync(It.Is<SessionResponse>(s => s.UserId == 1001), TimeSpan.FromMinutes(30)), Times.Once);
        _repository.Verify(r => r.AddToActiveSetAsync(result.SessionId), Times.Once);
        _activityTracking.Verify(a => a.TrackActivityAsync(1001, ActivityType.Login), Times.Once);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Users, It.Is<UserLoggedInEvent>(e => e.Payload.UserId == 1001)), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ExistingActiveSession_ReusesSessionInsteadOfCreatingDuplicate()
    {
        var existing = new SessionResponse("existing-session", 1001, DateTime.UtcNow, DateTime.UtcNow, "127.0.0.1", "Chrome", "Desktop");
        _repository.Setup(r => r.GetActiveSessionIdForUserAsync(1001)).ReturnsAsync("existing-session");
        _repository.Setup(r => r.GetAsync("existing-session")).ReturnsAsync(existing);

        var result = await _sut.LoginAsync(new LoginRequest(1001), "127.0.0.1", "Chrome/1.0");

        Assert.Equal("existing-session", result.SessionId);
        _repository.Verify(r => r.SaveAsync(It.IsAny<SessionResponse>(), It.IsAny<TimeSpan>()), Times.Never);
        _repository.Verify(r => r.RefreshAsync("existing-session", TimeSpan.FromMinutes(30)), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_PreviousSessionExpired_CreatesNewSessionInsteadOfReturningStaleMapping()
    {
        _repository.Setup(r => r.GetActiveSessionIdForUserAsync(1001)).ReturnsAsync("stale-session");
        _repository.Setup(r => r.GetAsync("stale-session")).ReturnsAsync((SessionResponse?)null);

        var result = await _sut.LoginAsync(new LoginRequest(1001), "127.0.0.1", "Chrome/1.0");

        Assert.NotEqual("stale-session", result.SessionId);
        _repository.Verify(r => r.SaveAsync(It.IsAny<SessionResponse>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_RemovesSessionAndActiveSetEntry()
    {
        var session = new SessionResponse("session-1", 1001, DateTime.UtcNow, DateTime.UtcNow, "127.0.0.1", "Chrome", "Desktop");
        _repository.Setup(r => r.GetAsync("session-1")).ReturnsAsync(session);

        await _sut.LogoutAsync("session-1");

        _repository.Verify(r => r.DeleteAsync("session-1"), Times.Once);
        _repository.Verify(r => r.RemoveFromActiveSetAsync("session-1"), Times.Once);
        _repository.Verify(r => r.RemoveUserSessionMappingAsync(1001), Times.Once);
        _publisher.Verify(p => p.PublishAsync(RedisChannels.Users, It.Is<UserLoggedOutEvent>(e => e.Payload.UserId == 1001 && e.Payload.SessionId == "session-1")), Times.Once);
    }

    [Fact]
    public async Task RefreshSessionAsync_ValidSession_TouchesTtlWithoutRewritingValue()
    {
        var session = new SessionResponse("session-1", 1001, DateTime.UtcNow, DateTime.UtcNow, "127.0.0.1", "Chrome", "Desktop");
        _repository.Setup(r => r.GetAsync("session-1")).ReturnsAsync(session);

        var result = await _sut.RefreshSessionAsync("session-1");

        Assert.Equal(session, result);
        _repository.Verify(r => r.RefreshAsync("session-1", TimeSpan.FromMinutes(30)), Times.Once);
        _repository.Verify(r => r.SaveAsync(It.IsAny<SessionResponse>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task RefreshSessionAsync_ExpiredSession_ReturnsNull()
    {
        _repository.Setup(r => r.GetAsync("gone")).ReturnsAsync((SessionResponse?)null);

        var result = await _sut.RefreshSessionAsync("gone");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveSessionsAsync_SkipsSessionsMissingFromCache()
    {
        _repository.Setup(r => r.GetActiveSessionIdsAsync()).ReturnsAsync(["s1", "s2"]);
        _repository.Setup(r => r.GetAsync("s1")).ReturnsAsync(new SessionResponse("s1", 1, DateTime.UtcNow, DateTime.UtcNow, "1.1.1.1", "Chrome", "Desktop"));
        _repository.Setup(r => r.GetAsync("s2")).ReturnsAsync((SessionResponse?)null);

        var result = await _sut.GetActiveSessionsAsync();

        Assert.Single(result);
        Assert.Equal("s1", result[0].SessionId);
    }
}
