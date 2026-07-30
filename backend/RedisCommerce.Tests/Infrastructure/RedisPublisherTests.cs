using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Events;
using RedisCommerce.Infrastructure.Messaging;
using StackExchange.Redis;
using Xunit;

namespace RedisCommerce.Tests.Infrastructure;

public class RedisPublisherTests
{
    private readonly Mock<ISubscriber> _subscriber = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly RedisPublisher _sut;

    public RedisPublisherTests()
    {
        _connectionMultiplexer
            .Setup(m => m.GetSubscriber(It.IsAny<object>()))
            .Returns(_subscriber.Object);

        _sut = new RedisPublisher(_connectionMultiplexer.Object, Mock.Of<ILogger<RedisPublisher>>());
    }

    [Fact]
    public async Task PublishAsync_SerializesEventAsJsonAndPublishesToTheGivenChannel()
    {
        _subscriber
            .Setup(s => s.PublishAsync(
                It.Is<RedisChannel>(c => c.ToString() == RedisChannels.Products),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(3L);

        var domainEvent = new ProductCreatedEvent
        {
            Payload = new ProductCreatedPayload(1, "Widget", 9.99m, 10),
        };

        var result = await _sut.PublishAsync(RedisChannels.Products, domainEvent);

        Assert.Equal(3L, result);
        _subscriber.Verify(s => s.PublishAsync(
            It.Is<RedisChannel>(c => c.ToString() == RedisChannels.Products),
            It.Is<RedisValue>(v => v.ToString().Contains("\"ProductId\":1") && v.ToString().Contains(domainEvent.EventId.ToString())),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }
}
