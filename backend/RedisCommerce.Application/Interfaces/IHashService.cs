namespace RedisCommerce.Application.Interfaces;

public interface IHashService
{
    Task HashSetAsync(string key, string field, string value);
    Task<string?> HashGetAsync(string key, string field);
    Task<IDictionary<string, string>> HashGetAllAsync(string key);
    Task<bool> HashDeleteAsync(string key, string field);
    Task<bool> HashExistsAsync(string key, string field);
    Task<long> HashLengthAsync(string key);
    Task<bool> ExpireAsync(string key, TimeSpan expiration);
    Task<bool> DeleteAsync(string key);
}
