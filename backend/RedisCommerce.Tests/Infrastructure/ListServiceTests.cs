using Moq;
using RedisCommerce.Infrastructure.Caching;
using StackExchange.Redis;
using Xunit;

namespace RedisCommerce.Tests.Infrastructure;

public class ListServiceTests
{
    private readonly Mock<IDatabase> _database = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly ListService _sut;

    public ListServiceTests()
    {
        _connectionMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);

        _sut = new ListService(_connectionMultiplexer.Object);
    }

    [Fact]
    public async Task LeftPushAsync_CallsListLeftPush()
    {
        _database
            .Setup(d => d.ListLeftPushAsync("order-processing", "42", It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        var result = await _sut.LeftPushAsync("order-processing", "42");

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task RightPushAsync_CallsListRightPush()
    {
        _database
            .Setup(d => d.ListRightPushAsync("order-processing", "42", It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        var result = await _sut.RightPushAsync("order-processing", "42");

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task LeftPopAsync_QueueHasItems_ReturnsValue()
    {
        _database
            .Setup(d => d.ListLeftPopAsync("order-processing", It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)"42");

        var result = await _sut.LeftPopAsync("order-processing");

        Assert.Equal("42", result);
    }

    [Fact]
    public async Task RightPopAsync_QueueEmpty_ReturnsNull()
    {
        _database
            .Setup(d => d.ListRightPopAsync("order-processing", It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var result = await _sut.RightPopAsync("order-processing");

        Assert.Null(result);
    }

    [Fact]
    public async Task LengthAsync_ReturnsQueueDepth()
    {
        _database
            .Setup(d => d.ListLengthAsync("order-processing", It.IsAny<CommandFlags>()))
            .ReturnsAsync(5);

        var result = await _sut.LengthAsync("order-processing");

        Assert.Equal(5, result);
    }
}
