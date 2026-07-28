using System.Text.Json;
using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Application.Services;

public class ExpirationNotificationService : IExpirationNotificationService
{
    private const int MaxRecentEvents = 100;

    private readonly IListService _listService;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<ExpirationNotificationService> _logger;

    public ExpirationNotificationService(
        IListService listService,
        ISessionRepository sessionRepository,
        ILogger<ExpirationNotificationService> logger)
    {
        _listService = listService;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task HandleExpirationAsync(string expiredKey)
    {
        if (expiredKey.StartsWith(CacheKeys.SessionUserMappingPrefix, StringComparison.Ordinal))
        {
            // The reverse user->session pointer shares the session's TTL; its own expiry
            // isn't independently interesting — the session key expiring is the real event.
            return;
        }

        if (expiredKey.StartsWith(CacheKeys.SessionPrefix, StringComparison.Ordinal))
        {
            var sessionId = expiredKey[CacheKeys.SessionPrefix.Length..];
            await _sessionRepository.RemoveFromActiveSetAsync(sessionId);
            await RecordEventAsync(expiredKey, "Session Expired");
            _logger.LogInformation("Session Expired: {SessionId}", sessionId);
            return;
        }

        if (expiredKey.StartsWith(CacheKeys.CartPrefix, StringComparison.Ordinal))
        {
            var userId = expiredKey[CacheKeys.CartPrefix.Length..];
            await RecordEventAsync(expiredKey, "Cart Expired");
            _logger.LogInformation("Cart Expired: user {UserId}", userId);
        }
    }

    public async Task<IReadOnlyList<ExpirationEventResponse>> GetRecentEventsAsync(int count)
    {
        var entries = await _listService.RangeAsync(CacheKeys.ExpirationEvents, 0, count - 1);
        var events = new List<ExpirationEventResponse>(entries.Count);

        foreach (var json in entries)
        {
            ExpirationEventResponse? parsed = null;
            try
            {
                parsed = JsonSerializer.Deserialize<ExpirationEventResponse>(json);
            }
            catch (JsonException)
            {
                _logger.LogWarning("Skipping malformed expiration event payload");
            }

            if (parsed is not null)
            {
                events.Add(parsed);
            }
        }

        return events;
    }

    private async Task RecordEventAsync(string key, string eventType)
    {
        var eventRecord = new ExpirationEventResponse(key, eventType, DateTime.UtcNow);
        await _listService.LeftPushAsync(CacheKeys.ExpirationEvents, JsonSerializer.Serialize(eventRecord));
        await _listService.TrimAsync(CacheKeys.ExpirationEvents, 0, MaxRecentEvents - 1);
    }
}
