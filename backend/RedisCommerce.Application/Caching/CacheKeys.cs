namespace RedisCommerce.Application.Caching;

public static class CacheKeys
{
    public const string PopularProducts = "popular-products";
    public const string OrderProcessingQueue = "order-processing";
    public const string ActiveSessions = "sessions:active";
    public const string ExpirationEvents = "expiration-events";
    public const string DailyActiveCounts = "analytics:daily-active-counts";

    public const string SessionPrefix = "session:";
    public const string SessionUserMappingPrefix = "session:user:";
    public const string CartPrefix = "cart:";

    public static string Product(int id) => $"product:{id}";
    public static string Cart(int userId) => $"{CartPrefix}{userId}";
    public static string Favorites(int userId) => $"favorites:{userId}";

    public static string Session(string sessionId) => $"{SessionPrefix}{sessionId}";
    public static string SessionUserMapping(int userId) => $"{SessionUserMappingPrefix}{userId}";

    public static string Activity(DateOnly date) => $"activity:{date:yyyyMMdd}";

    public static string VisitorsDaily(DateOnly date) => $"visitors:daily:{date:yyyyMMdd}";
    public static string VisitorsWeekly(int isoYear, int isoWeek) => $"visitors:weekly:{isoYear}W{isoWeek:D2}";
    public static string VisitorsMonthly(int year, int month) => $"visitors:monthly:{year}{month:D2}";
}
