namespace RedisCommerce.Application.Caching;

public static class CacheKeys
{
    public const string PopularProducts = "popular-products";
    public const string OrderProcessingQueue = "order-processing";
    public const string ActiveSessions = "sessions:active";

    public static string Product(int id) => $"product:{id}";
    public static string Cart(int userId) => $"cart:{userId}";
    public static string Favorites(int userId) => $"favorites:{userId}";

    public static string Session(string sessionId) => $"session:{sessionId}";
    public static string SessionUserMapping(int userId) => $"session:user:{userId}";

    public static string Activity(DateOnly date) => $"activity:{date:yyyyMMdd}";
}
