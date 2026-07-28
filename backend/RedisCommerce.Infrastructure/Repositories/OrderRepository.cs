using Microsoft.EntityFrameworkCore;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Domain.Entities;
using RedisCommerce.Infrastructure.Data;

namespace RedisCommerce.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly RedisCommerceDbContext _context;

    public OrderRepository(RedisCommerceDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id) =>
        await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }
}
