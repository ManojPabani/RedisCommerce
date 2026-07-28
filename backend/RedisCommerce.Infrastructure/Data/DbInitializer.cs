using Microsoft.EntityFrameworkCore;
using RedisCommerce.Domain.Entities;

namespace RedisCommerce.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(RedisCommerceDbContext context)
    {
        if (await context.Products.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        context.Products.AddRange(
            new Product
            {
                Name = "Mechanical Keyboard",
                Description = "RGB backlit mechanical keyboard with blue switches.",
                Price = 79.99m,
                StockQuantity = 150,
                CreatedDate = now,
                UpdatedDate = now,
            },
            new Product
            {
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with adjustable DPI.",
                Price = 29.99m,
                StockQuantity = 300,
                CreatedDate = now,
                UpdatedDate = now,
            },
            new Product
            {
                Name = "27-inch 4K Monitor",
                Description = "27-inch UHD monitor with HDR support.",
                Price = 349.99m,
                StockQuantity = 75,
                CreatedDate = now,
                UpdatedDate = now,
            },
            new Product
            {
                Name = "USB-C Docking Station",
                Description = "11-in-1 docking station with dual HDMI and 100W PD.",
                Price = 89.99m,
                StockQuantity = 120,
                CreatedDate = now,
                UpdatedDate = now,
            },
            new Product
            {
                Name = "Noise Cancelling Headphones",
                Description = "Over-ear headphones with active noise cancellation.",
                Price = 199.99m,
                StockQuantity = 90,
                CreatedDate = now,
                UpdatedDate = now,
            });

        await context.SaveChangesAsync();
    }
}
