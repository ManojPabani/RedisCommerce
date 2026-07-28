using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Interfaces;

public interface ISessionService
{
    Task<SessionResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent);
    Task LogoutAsync(string sessionId);
    Task<SessionResponse?> GetSessionAsync(string sessionId);
    Task<SessionResponse?> RefreshSessionAsync(string sessionId);
    Task<IReadOnlyList<SessionResponse>> GetActiveSessionsAsync();
}
