using System.Text.Json;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Repositories;

public class RedisSessionRepository : ISessionRepository
{
    private readonly IRedisCacheService _cache;
    private readonly ISetService _setService;

    public RedisSessionRepository(IRedisCacheService cache, ISetService setService)
    {
        _cache = cache;
        _setService = setService;
    }

    public async Task SaveAsync(SessionResponse session, TimeSpan ttl)
    {
        await _cache.SetAsync(CacheKeys.Session(session.SessionId), JsonSerializer.Serialize(session), ttl);
    }

    public async Task<SessionResponse?> GetAsync(string sessionId)
    {
        var json = await _cache.GetAsync(CacheKeys.Session(sessionId));
        return json is null ? null : JsonSerializer.Deserialize<SessionResponse>(json);
    }

    public async Task<bool> RefreshAsync(string sessionId, TimeSpan ttl) =>
        await _cache.RefreshExpirationAsync(CacheKeys.Session(sessionId), ttl);

    public async Task DeleteAsync(string sessionId)
    {
        await _cache.RemoveAsync(CacheKeys.Session(sessionId));
    }

    public async Task<string?> GetActiveSessionIdForUserAsync(int userId) =>
        await _cache.GetAsync(CacheKeys.SessionUserMapping(userId));

    public async Task SetUserSessionMappingAsync(int userId, string sessionId, TimeSpan ttl)
    {
        await _cache.SetAsync(CacheKeys.SessionUserMapping(userId), sessionId, ttl);
    }

    public async Task RemoveUserSessionMappingAsync(int userId)
    {
        await _cache.RemoveAsync(CacheKeys.SessionUserMapping(userId));
    }

    public async Task AddToActiveSetAsync(string sessionId)
    {
        await _setService.AddAsync(CacheKeys.ActiveSessions, sessionId);
    }

    public async Task RemoveFromActiveSetAsync(string sessionId)
    {
        await _setService.RemoveAsync(CacheKeys.ActiveSessions, sessionId);
    }

    public async Task<IReadOnlyList<string>> GetActiveSessionIdsAsync() =>
        await _setService.MembersAsync(CacheKeys.ActiveSessions);
}
