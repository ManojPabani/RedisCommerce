using Moq;
using RedisCommerce.Infrastructure.Caching;
using StackExchange.Redis;
using Xunit;

namespace RedisCommerce.Tests.Infrastructure;

public class HashServiceTests
{
    private readonly Mock<IDatabase> _database = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly HashService _sut;

    public HashServiceTests()
    {
        _connectionMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);

        _sut = new HashService(_connectionMultiplexer.Object);
    }

    [Fact]
    public async Task HashSetAsync_CallsHashSet()
    {
        await _sut.HashSetAsync("cart:1001", "10", "2");

        _database.Verify(d => d.HashSetAsync(
            "cart:1001", "10", "2", It.IsAny<When>(), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task HashGetAsync_FieldExists_ReturnsValue()
    {
        _database
            .Setup(d => d.HashGetAsync("cart:1001", "10", It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)"2");

        var result = await _sut.HashGetAsync("cart:1001", "10");

        Assert.Equal("2", result);
    }

    [Fact]
    public async Task HashGetAsync_FieldMissing_ReturnsNull()
    {
        _database
            .Setup(d => d.HashGetAsync("cart:1001", "10", It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var result = await _sut.HashGetAsync("cart:1001", "10");

        Assert.Null(result);
    }

    [Fact]
    public async Task HashGetAllAsync_ReturnsAllFieldsAsDictionary()
    {
        _database
            .Setup(d => d.HashGetAllAsync("cart:1001", It.IsAny<CommandFlags>()))
            .ReturnsAsync([new HashEntry("10", "2"), new HashEntry("50", "5")]);

        var result = await _sut.HashGetAllAsync("cart:1001");

        Assert.Equal(2, result.Count);
        Assert.Equal("2", result["10"]);
        Assert.Equal("5", result["50"]);
    }

    [Fact]
    public async Task HashDeleteAsync_CallsHashDelete()
    {
        _database
            .Setup(d => d.HashDeleteAsync("cart:1001", "10", It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.HashDeleteAsync("cart:1001", "10");

        Assert.True(result);
    }

    [Fact]
    public async Task HashExistsAsync_CallsHashExists()
    {
        _database
            .Setup(d => d.HashExistsAsync("cart:1001", "10", It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.HashExistsAsync("cart:1001", "10");

        Assert.True(result);
    }

    [Fact]
    public async Task HashLengthAsync_ReturnsFieldCount()
    {
        _database
            .Setup(d => d.HashLengthAsync("cart:1001", It.IsAny<CommandFlags>()))
            .ReturnsAsync(3);

        var result = await _sut.HashLengthAsync("cart:1001");

        Assert.Equal(3, result);
    }

    [Fact]
    public async Task ExpireAsync_CallsKeyExpire()
    {
        var ttl = TimeSpan.FromHours(24);

        await _sut.ExpireAsync("cart:1001", ttl);

        _database.Verify(d => d.KeyExpireAsync(
            "cart:1001", ttl, It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsKeyDelete()
    {
        await _sut.DeleteAsync("cart:1001");

        _database.Verify(d => d.KeyDeleteAsync("cart:1001", It.IsAny<CommandFlags>()), Times.Once);
    }
}
