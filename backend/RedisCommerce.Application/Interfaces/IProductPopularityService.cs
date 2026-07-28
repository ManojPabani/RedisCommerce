using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Interfaces;

public interface IProductPopularityService
{
    Task RecordViewAsync(int productId);
    Task<IReadOnlyList<PopularProductResponse>> GetTopProductsAsync(int count);
}
