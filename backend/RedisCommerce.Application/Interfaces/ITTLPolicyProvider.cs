namespace RedisCommerce.Application.Interfaces;

public enum RedisObjectType
{
    Product,
    Cart,
    Session,
    Analytics,
    Popularity,
    Favorites,
}

/// <summary>
/// Central lookup for how long each kind of Redis-backed object should live.
/// A null result means the object should never expire.
/// </summary>
public interface ITTLPolicyProvider
{
    TimeSpan? GetTtl(RedisObjectType type);
}
