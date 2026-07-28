namespace RedisCommerce.Application.Caching;

public static class CacheKeys
{
    public const string PopularProducts = "popular-products";
    public const string OrderProcessingQueue = "order-processing";

    public static string Product(int id) => $"product:{id}";
    public static string Cart(int userId) => $"cart:{userId}";
    public static string Favorites(int userId) => $"favorites:{userId}";
}
