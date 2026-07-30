using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Events;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Application.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _repository;
    private readonly ITTLPolicyProvider _ttlPolicy;
    private readonly IActivityTrackingService _activityTracking;
    private readonly IRedisPublisher _publisher;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        ISessionRepository repository,
        ITTLPolicyProvider ttlPolicy,
        IActivityTrackingService activityTracking,
        IRedisPublisher publisher,
        ILogger<SessionService> logger)
    {
        _repository = repository;
        _ttlPolicy = ttlPolicy;
        _activityTracking = activityTracking;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<SessionResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent)
    {
        var ttl = _ttlPolicy.GetTtl(RedisObjectType.Session)!.Value;

        var existingSessionId = await _repository.GetActiveSessionIdForUserAsync(request.UserId);
        if (existingSessionId is not null)
        {
            var existingSession = await _repository.GetAsync(existingSessionId);
            if (existingSession is not null)
            {
                await _repository.RefreshAsync(existingSessionId, ttl);
                await _repository.SetUserSessionMappingAsync(request.UserId, existingSessionId, ttl);
                await _activityTracking.TrackActivityAsync(request.UserId, ActivityType.Login);
                _logger.LogInformation("Session Refreshed: {SessionId} (reused on re-login)", existingSessionId);
                await PublishUserLoggedInAsync(request.UserId, existingSessionId);
                return existingSession;
            }
        }

        var now = DateTime.UtcNow;
        var (browser, device) = ParseUserAgent(userAgent);
        var session = new SessionResponse(
            SessionId: Guid.NewGuid().ToString("N"),
            UserId: request.UserId,
            LoginTime: now,
            LastActivity: now,
            IpAddress: ipAddress,
            Browser: browser,
            Device: device);

        await _repository.SaveAsync(session, ttl);
        await _repository.SetUserSessionMappingAsync(request.UserId, session.SessionId, ttl);
        await _repository.AddToActiveSetAsync(session.SessionId);
        await _activityTracking.TrackActivityAsync(request.UserId, ActivityType.Login);

        _logger.LogInformation("Session Created: {SessionId} for user {UserId}", session.SessionId, request.UserId);
        await PublishUserLoggedInAsync(request.UserId, session.SessionId);

        return session;
    }

    public async Task LogoutAsync(string sessionId)
    {
        var session = await _repository.GetAsync(sessionId);

        await _repository.DeleteAsync(sessionId);
        await _repository.RemoveFromActiveSetAsync(sessionId);

        if (session is not null)
        {
            await _repository.RemoveUserSessionMappingAsync(session.UserId);
        }

        _logger.LogInformation("Session logged out: {SessionId}", sessionId);

        if (session is not null)
        {
            await _publisher.PublishAsync(RedisChannels.Users, new UserLoggedOutEvent
            {
                Payload = new UserLoggedOutPayload(session.UserId, sessionId),
            });
        }
    }

    private async Task PublishUserLoggedInAsync(int userId, string sessionId)
    {
        await _publisher.PublishAsync(RedisChannels.Users, new UserLoggedInEvent
        {
            Payload = new UserLoggedInPayload(userId, sessionId),
        });
    }

    public async Task<SessionResponse?> GetSessionAsync(string sessionId) =>
        await _repository.GetAsync(sessionId);

    public async Task<SessionResponse?> RefreshSessionAsync(string sessionId)
    {
        var session = await _repository.GetAsync(sessionId);
        if (session is null)
        {
            return null;
        }

        var ttl = _ttlPolicy.GetTtl(RedisObjectType.Session)!.Value;
        await _repository.RefreshAsync(sessionId, ttl);
        await _repository.SetUserSessionMappingAsync(session.UserId, sessionId, ttl);

        _logger.LogInformation("Session Refreshed: {SessionId}", sessionId);

        return session;
    }

    public async Task<IReadOnlyList<SessionResponse>> GetActiveSessionsAsync()
    {
        var sessionIds = await _repository.GetActiveSessionIdsAsync();

        var sessions = new List<SessionResponse>();
        foreach (var sessionId in sessionIds)
        {
            var session = await _repository.GetAsync(sessionId);
            if (session is not null)
            {
                sessions.Add(session);
            }
        }

        return sessions;
    }

    private static (string Browser, string Device) ParseUserAgent(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return ("Unknown", "Unknown");
        }

        var browser = userAgent switch
        {
            var ua when ua.Contains("Edg/") => "Edge",
            var ua when ua.Contains("Chrome/") => "Chrome",
            var ua when ua.Contains("Firefox/") => "Firefox",
            var ua when ua.Contains("Safari/") && !ua.Contains("Chrome/") => "Safari",
            _ => "Other",
        };

        var device = userAgent switch
        {
            var ua when ua.Contains("Mobile") => "Mobile",
            var ua when ua.Contains("Tablet") => "Tablet",
            _ => "Desktop",
        };

        return (browser, device);
    }
}
