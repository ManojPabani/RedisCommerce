using System.Text.Json;
using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Events;
using RedisCommerce.Application.Interfaces;
using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Messaging;

public class RedisPublisher : IRedisPublisher
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisPublisher> _logger;

    public RedisPublisher(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisPublisher> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    public async Task<long> PublishAsync<TEvent>(string channel, TEvent domainEvent) where TEvent : BaseDomainEvent
    {
        var json = JsonSerializer.Serialize(domainEvent);
        var subscriber = _connectionMultiplexer.GetSubscriber();
        var redisChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);

        var receiverCount = await subscriber.PublishAsync(redisChannel, json);

        _logger.LogInformation(
            "Event Published: {EventType} ({EventId}) -> {Channel} ({ReceiverCount} subscriber(s))",
            domainEvent.EventType,
            domainEvent.EventId,
            channel,
            receiverCount);

        return receiverCount;
    }
}
