namespace RedisCommerce.Application.Interfaces;

public interface IListService
{
    Task<long> LeftPushAsync(string key, string value);
    Task<long> RightPushAsync(string key, string value);
    Task<string?> LeftPopAsync(string key);
    Task<string?> RightPopAsync(string key);
    Task<long> LengthAsync(string key);
    Task<IReadOnlyList<string>> RangeAsync(string key, long start, long stop);
    Task TrimAsync(string key, long start, long stop);
}
