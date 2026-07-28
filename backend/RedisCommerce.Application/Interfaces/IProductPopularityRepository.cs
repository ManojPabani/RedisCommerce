namespace RedisCommerce.Application.Interfaces;

public interface IProductPopularityRepository
{
    Task<double> IncrementAsync(int productId);
    Task<IReadOnlyList<ScoredMember>> GetTopAsync(int count);
}
