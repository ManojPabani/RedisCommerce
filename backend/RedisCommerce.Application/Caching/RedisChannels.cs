namespace RedisCommerce.Application.Caching;

/// <summary>
/// Single source of truth for Redis Pub/Sub channel names (Phase 4 general-purpose business events —
/// distinct from the keyspace-notification channel used by RedisExpirationListener).
/// </summary>
public static class RedisChannels
{
    public const string Products = "events:products";
    public const string Inventory = "events:inventory";
    public const string Orders = "events:orders";
    public const string Cart = "events:cart";
    public const string Users = "events:users";
    public const string Sessions = "events:sessions";
    public const string Analytics = "events:analytics";
    public const string Notifications = "events:notifications";
}
