using Microsoft.EntityFrameworkCore;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Domain.Entities;
using RedisCommerce.Infrastructure.Data;

namespace RedisCommerce.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly RedisCommerceDbContext _context;

    public ProductRepository(RedisCommerceDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id) =>
        await _context.Products.FindAsync(id);

    public async Task<IReadOnlyDictionary<int, Product>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return new Dictionary<int, Product>();
        }

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => idList.Contains(p.Id))
            .ToListAsync();

        return products.ToDictionary(p => p.Id);
    }

    public async Task<IEnumerable<Product>> GetAllAsync() =>
        await _context.Products.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Product>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        return await _context.Products
            .AsNoTracking()
            .Where(p => EF.Functions.Like(p.Name, $"%{query}%"))
            .ToListAsync();
    }

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}
