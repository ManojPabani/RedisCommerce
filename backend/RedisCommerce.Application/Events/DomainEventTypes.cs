namespace RedisCommerce.Application.Events;

/// <summary>Constant EventType strings so channel payloads never rely on a hardcoded literal or typeof(...).Name.</summary>
public static class DomainEventTypes
{
    public const string ProductCreated = "ProductCreated";
    public const string ProductUpdated = "ProductUpdated";
    public const string ProductDeleted = "ProductDeleted";
    public const string PriceChanged = "PriceChanged";

    public const string InventoryUpdated = "InventoryUpdated";

    public const string OrderCreated = "OrderCreated";
    public const string OrderStatusChanged = "OrderStatusChanged";
    public const string OrderCancelled = "OrderCancelled";

    public const string CartUpdated = "CartUpdated";
    public const string CartCheckedOut = "CartCheckedOut";

    public const string UserLoggedIn = "UserLoggedIn";
    public const string UserLoggedOut = "UserLoggedOut";
    public const string FavoriteAdded = "FavoriteAdded";
    public const string FavoriteRemoved = "FavoriteRemoved";

    public const string SessionExpired = "SessionExpired";

    public const string AnalyticsGenerated = "AnalyticsGenerated";

    public const string NotificationCreated = "NotificationCreated";
}
