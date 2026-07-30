using RedisCommerce.Application.Events;

namespace RedisCommerce.Application.Interfaces;

/// <summary>Publishes strongly typed domain events as JSON to a Redis Pub/Sub channel.</summary>
public interface IRedisPublisher
{
    Task<long> PublishAsync<TEvent>(string channel, TEvent domainEvent) where TEvent : BaseDomainEvent;
}
