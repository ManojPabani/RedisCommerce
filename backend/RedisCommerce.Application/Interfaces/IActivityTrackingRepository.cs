namespace RedisCommerce.Application.Interfaces;

public interface IActivityTrackingRepository
{
    Task MarkActiveAsync(DateOnly date, int userId);
    Task<long> GetActiveCountAsync(DateOnly date);
    Task<bool> IsActiveAsync(DateOnly date, int userId);
    Task<long> GetUniqueCountOverRangeAsync(IReadOnlyList<DateOnly> dates);
}
