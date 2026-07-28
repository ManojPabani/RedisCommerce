using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse> GetByIdAsync(int id);
    Task<IEnumerable<ProductResponse>> GetAllAsync();
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
}
