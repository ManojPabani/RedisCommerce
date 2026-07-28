namespace RedisCommerce.Application.Interfaces;

public interface IRedisCacheService
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan expiration);
    Task RemoveAsync(string key);
    Task<bool> RefreshExpirationAsync(string key, TimeSpan expiration);
}
