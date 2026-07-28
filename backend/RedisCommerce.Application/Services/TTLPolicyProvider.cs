using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisCommerce.Application.Configuration;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Application.Services;

public class TTLPolicyProvider : ITTLPolicyProvider
{
    private readonly RedisOptions _options;
    private readonly ILogger<TTLPolicyProvider> _logger;

    public TTLPolicyProvider(IOptions<RedisOptions> options, ILogger<TTLPolicyProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public TimeSpan? GetTtl(RedisObjectType type)
    {
        var ttl = type switch
        {
            RedisObjectType.Product => TimeSpan.FromMinutes(_options.ProductTTLMinutes),
            RedisObjectType.Cart => TimeSpan.FromHours(_options.CartTTLHours),
            RedisObjectType.Session => TimeSpan.FromMinutes(_options.SessionTTLMinutes),
            RedisObjectType.Analytics => TimeSpan.FromDays(_options.AnalyticsTTLDays),
            RedisObjectType.Popularity => (TimeSpan?)null,
            RedisObjectType.Favorites => (TimeSpan?)null,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

        _logger.LogInformation("TTL Applied: {ObjectType} -> {Ttl}", type, ttl?.ToString() ?? "never");

        return ttl;
    }
}
