using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse> GetByIdAsync(int id, int? viewerUserId = null, string? visitorId = null);
    Task<IEnumerable<ProductResponse>> GetAllAsync();
    Task<IEnumerable<ProductResponse>> SearchAsync(string query, int? viewerUserId = null);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
}
