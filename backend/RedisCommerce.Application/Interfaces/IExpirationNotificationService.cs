using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Interfaces;

public interface IExpirationNotificationService
{
    Task HandleExpirationAsync(string expiredKey);
    Task<IReadOnlyList<ExpirationEventResponse>> GetRecentEventsAsync(int count);
}
