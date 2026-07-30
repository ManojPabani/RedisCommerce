using RedisCommerce.Application.Events;

namespace RedisCommerce.Application.Interfaces;

/// <summary>Subscribes to a Redis Pub/Sub channel and deserializes incoming messages into a strongly typed event.</summary>
public interface IRedisSubscriber
{
    Task SubscribeAsync<TEvent>(string channel, Func<TEvent, Task> handler) where TEvent : BaseDomainEvent;

    Task UnsubscribeAsync(string channel);
}
