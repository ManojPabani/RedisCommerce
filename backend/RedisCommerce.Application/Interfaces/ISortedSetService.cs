namespace RedisCommerce.Application.Interfaces;

public record ScoredMember(string Member, double Score);

public interface ISortedSetService
{
    Task<bool> AddAsync(string key, string member, double score);
    Task<double> IncrementAsync(string key, string member, double increment);
    Task<IReadOnlyList<ScoredMember>> RangeDescendingAsync(string key, long start, long stop);
    Task<double?> ScoreAsync(string key, string member);
    Task<bool> RemoveAsync(string key, string member);
}
