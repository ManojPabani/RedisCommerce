using System.Text.Json;
using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Events;
using RedisCommerce.Application.Interfaces;
using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Messaging;

public class RedisSubscriber : IRedisSubscriber
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisSubscriber> _logger;

    public RedisSubscriber(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisSubscriber> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    public async Task SubscribeAsync<TEvent>(string channel, Func<TEvent, Task> handler) where TEvent : BaseDomainEvent
    {
        var subscriber = _connectionMultiplexer.GetSubscriber();
        var redisChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);

        await subscriber.SubscribeAsync(redisChannel, (_, message) => OnMessage(channel, message, handler));

        _logger.LogInformation("Subscribed to {Channel} for {EventType}", channel, typeof(TEvent).Name);
    }

    public async Task UnsubscribeAsync(string channel)
    {
        var subscriber = _connectionMultiplexer.GetSubscriber();
        var redisChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);

        await subscriber.UnsubscribeAsync(redisChannel);

        _logger.LogInformation("Unsubscribed from {Channel}", channel);
    }

    private void OnMessage<TEvent>(string channel, RedisValue message, Func<TEvent, Task> handler) where TEvent : BaseDomainEvent
    {
        _ = HandleMessageSafelyAsync(channel, message, handler);
    }

    private async Task HandleMessageSafelyAsync<TEvent>(string channel, RedisValue message, Func<TEvent, Task> handler) where TEvent : BaseDomainEvent
    {
        try
        {
            var domainEvent = JsonSerializer.Deserialize<TEvent>((string)message!);
            if (domainEvent is null)
            {
                _logger.LogWarning("Received an empty or malformed event on {Channel}; ignoring", channel);
                return;
            }

            _logger.LogInformation(
                "Event Received: {EventType} ({EventId}) <- {Channel}",
                domainEvent.EventType,
                domainEvent.EventId,
                channel);

            await handler(domainEvent);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Serialization failure deserializing event on {Channel}; message discarded", channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing event on {Channel}", channel);
        }
    }
}
