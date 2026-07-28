namespace RedisCommerce.Application.Interfaces;

public interface IHyperLogLogService
{
    Task<bool> AddAsync(string key, string value);
    Task<long> CountAsync(params string[] keys);
    Task MergeAsync(string destinationKey, params string[] sourceKeys);
}
