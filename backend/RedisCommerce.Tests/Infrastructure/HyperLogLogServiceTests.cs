using Moq;
using RedisCommerce.Infrastructure.Caching;
using StackExchange.Redis;
using Xunit;

namespace RedisCommerce.Tests.Infrastructure;

public class HyperLogLogServiceTests
{
    private readonly Mock<IDatabase> _database = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly HyperLogLogService _sut;

    public HyperLogLogServiceTests()
    {
        _connectionMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);

        _sut = new HyperLogLogService(_connectionMultiplexer.Object);
    }

    [Fact]
    public async Task AddAsync_CallsHyperLogLogAdd()
    {
        _database
            .Setup(d => d.HyperLogLogAddAsync("visitors:daily:20260728", "visitor-1", It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.AddAsync("visitors:daily:20260728", "visitor-1");

        Assert.True(result);
    }

    [Fact]
    public async Task CountAsync_SingleKey_ReturnsApproximateCardinality()
    {
        _database
            .Setup(d => d.HyperLogLogLengthAsync(It.Is<RedisKey[]>(k => k.Length == 1 && k[0] == "visitors:daily:20260728"), It.IsAny<CommandFlags>()))
            .ReturnsAsync(150);

        var result = await _sut.CountAsync("visitors:daily:20260728");

        Assert.Equal(150, result);
    }

    [Fact]
    public async Task CountAsync_MultipleKeys_ReturnsUnionCardinality()
    {
        _database
            .Setup(d => d.HyperLogLogLengthAsync(It.Is<RedisKey[]>(k => k.Length == 2), It.IsAny<CommandFlags>()))
            .ReturnsAsync(300);

        var result = await _sut.CountAsync("visitors:daily:20260727", "visitors:daily:20260728");

        Assert.Equal(300, result);
    }

    [Fact]
    public async Task MergeAsync_CallsHyperLogLogMerge()
    {
        await _sut.MergeAsync("visitors:weekly:2026W31", "visitors:daily:20260727", "visitors:daily:20260728");

        _database.Verify(d => d.HyperLogLogMergeAsync(
            "visitors:weekly:2026W31",
            It.Is<RedisKey[]>(k => k.Length == 2),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }
}
