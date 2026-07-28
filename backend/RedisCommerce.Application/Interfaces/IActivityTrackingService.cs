namespace RedisCommerce.Application.Interfaces;

public enum ActivityType
{
    ProductView,
    Login,
    Checkout,
    Search,
}

public interface IActivityTrackingService
{
    Task TrackActivityAsync(int userId, ActivityType type);
    Task<long> GetTodayCountAsync();
    Task<long> GetCountForDateAsync(DateOnly date);
    Task<long> GetLastNDaysUniqueCountAsync(int days);
    Task<bool> IsUserActiveTodayAsync(int userId);
}
