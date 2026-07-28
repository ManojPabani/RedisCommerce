using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Interfaces;

public interface ISessionRepository
{
    Task SaveAsync(SessionResponse session, TimeSpan ttl);
    Task<SessionResponse?> GetAsync(string sessionId);
    Task<bool> RefreshAsync(string sessionId, TimeSpan ttl);
    Task DeleteAsync(string sessionId);

    Task<string?> GetActiveSessionIdForUserAsync(int userId);
    Task SetUserSessionMappingAsync(int userId, string sessionId, TimeSpan ttl);
    Task RemoveUserSessionMappingAsync(int userId);

    Task AddToActiveSetAsync(string sessionId);
    Task RemoveFromActiveSetAsync(string sessionId);
    Task<IReadOnlyList<string>> GetActiveSessionIdsAsync();
}
