using Microsoft.Extensions.Logging;
using Moq;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.Events;
using RedisCommerce.Infrastructure.Messaging;
using StackExchange.Redis;
using Xunit;

namespace RedisCommerce.Tests.Infrastructure;

public class RedisSubscriberTests
{
    private readonly Mock<ISubscriber> _subscriber = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly RedisSubscriber _sut;

    public RedisSubscriberTests()
    {
        _connectionMultiplexer
            .Setup(m => m.GetSubscriber(It.IsAny<object>()))
            .Returns(_subscriber.Object);

        _sut = new RedisSubscriber(_connectionMultiplexer.Object, Mock.Of<ILogger<RedisSubscriber>>());
    }

    [Fact]
    public async Task SubscribeAsync_SubscribesToTheGivenChannel()
    {
        await _sut.SubscribeAsync<ProductCreatedEvent>(RedisChannels.Products, _ => Task.CompletedTask);

        _subscriber.Verify(s => s.SubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == RedisChannels.Products),
            It.IsAny<Action<RedisChannel, RedisValue>>(),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task UnsubscribeAsync_UnsubscribesFromTheGivenChannel()
    {
        await _sut.UnsubscribeAsync(RedisChannels.Orders);

        _subscriber.Verify(s => s.UnsubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == RedisChannels.Orders),
            It.IsAny<Action<RedisChannel, RedisValue>>(),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_DeserializesAndInvokesHandler_WhenAMessageArrives()
    {
        Action<RedisChannel, RedisValue>? capturedCallback = null;
        _subscriber
            .Setup(s => s.SubscribeAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<Action<RedisChannel, RedisValue>>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisChannel, Action<RedisChannel, RedisValue>, CommandFlags>((_, handler, _) => capturedCallback = handler)
            .Returns(Task.CompletedTask);

        ProductCreatedEvent? received = null;
        var handled = new TaskCompletionSource();

        await _sut.SubscribeAsync<ProductCreatedEvent>(RedisChannels.Products, evt =>
        {
            received = evt;
            handled.SetResult();
            return Task.CompletedTask;
        });

        var domainEvent = new ProductCreatedEvent { Payload = new ProductCreatedPayload(1, "Widget", 9.99m, 10) };
        var json = System.Text.Json.JsonSerializer.Serialize(domainEvent);

        capturedCallback!.Invoke(new RedisChannel(RedisChannels.Products, RedisChannel.PatternMode.Literal), json);

        await handled.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.NotNull(received);
        Assert.Equal(1, received!.Payload.ProductId);
    }
}
