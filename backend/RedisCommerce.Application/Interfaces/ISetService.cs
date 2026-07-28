namespace RedisCommerce.Application.Interfaces;

public interface ISetService
{
    Task<bool> AddAsync(string key, string member);
    Task<bool> RemoveAsync(string key, string member);
    Task<IReadOnlyList<string>> MembersAsync(string key);
    Task<bool> IsMemberAsync(string key, string member);
    Task<long> CardinalityAsync(string key);
}
