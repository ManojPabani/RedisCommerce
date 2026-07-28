using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Interfaces;
using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Workers;

/// <summary>
/// Subscribes to Redis keyspace notifications for expired keys so Session/Cart TTL expiry can be
/// observed and logged. This uses Redis's Pub/Sub subscribe mechanism under the hood (there is no
/// other way to observe key expiration) — scoped narrowly to expiration monitoring, distinct from
/// general-purpose business-event Pub/Sub which is deferred to a later phase.
/// Also fulfils the "Expired Session Monitor" hosted service.
/// </summary>
public class RedisExpirationListener : BackgroundService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RedisExpirationListener> _logger;

    public RedisExpirationListener(
        IConnectionMultiplexer connectionMultiplexer,
        IServiceScopeFactory scopeFactory,
        ILogger<RedisExpirationListener> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await TryEnableKeyspaceNotificationsAsync();

        var subscriber = _connectionMultiplexer.GetSubscriber();
        var channel = new RedisChannel("__keyevent@*__:expired", RedisChannel.PatternMode.Pattern);

        await subscriber.SubscribeAsync(channel, (_, key) => OnKeyExpired(key!));

        _logger.LogInformation("RedisExpirationListener subscribed to expired-key notifications");

        // Keep the hosted service alive for its lifetime; the subscription callback does the work.
        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
    }

    private async Task TryEnableKeyspaceNotificationsAsync()
    {
        try
        {
            var endpoint = _connectionMultiplexer.GetEndPoints().First();
            var server = _connectionMultiplexer.GetServer(endpoint);
            await server.ConfigSetAsync("notify-keyspace-events", "Ex");
            _logger.LogInformation("Enabled Redis keyspace notifications for expired events");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Could not set notify-keyspace-events (managed Redis providers often block CONFIG SET) — " +
                "expiration events will only be observed if it was already enabled on the server");
        }
    }

    private void OnKeyExpired(string expiredKey)
    {
        _ = HandleExpirationSafelyAsync(expiredKey);
    }

    private async Task HandleExpirationSafelyAsync(string expiredKey)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<IExpirationNotificationService>();
            await notificationService.HandleExpirationAsync(expiredKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle expiration notification for key {Key}", expiredKey);
        }
    }
}
