using Moq;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Infrastructure.Caching;
using StackExchange.Redis;
using Xunit;

namespace RedisCommerce.Tests.Infrastructure;

public class BitmapServiceTests
{
    private readonly Mock<IDatabase> _database = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly BitmapService _sut;

    public BitmapServiceTests()
    {
        _connectionMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);

        _sut = new BitmapService(_connectionMultiplexer.Object);
    }

    [Fact]
    public async Task SetBitAsync_CallsStringSetBit()
    {
        _database
            .Setup(d => d.StringSetBitAsync("activity:20260728", 1001, true, It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        var result = await _sut.SetBitAsync("activity:20260728", 1001);

        Assert.False(result);
    }

    [Fact]
    public async Task GetBitAsync_ReturnsBitValue()
    {
        _database
            .Setup(d => d.StringGetBitAsync("activity:20260728", 1001, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.GetBitAsync("activity:20260728", 1001);

        Assert.True(result);
    }

    [Fact]
    public async Task CountAsync_ReturnsBitCount()
    {
        _database
            .Setup(d => d.StringBitCountAsync("activity:20260728", It.IsAny<long>(), It.IsAny<long>(), It.IsAny<StringIndexType>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(42);

        var result = await _sut.CountAsync("activity:20260728");

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task BitPositionAsync_ReturnsFirstMatchingPosition()
    {
        _database
            .Setup(d => d.StringBitPositionAsync("activity:20260728", true, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<StringIndexType>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1001);

        var result = await _sut.BitPositionAsync("activity:20260728", true);

        Assert.Equal(1001, result);
    }

    [Fact]
    public async Task BitOpAsync_Or_CallsStringBitOperationWithSourceKeys()
    {
        _database
            .Setup(d => d.StringBitOperationAsync(Bitwise.Or, "activity:scratch", It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(8);

        var result = await _sut.BitOpAsync(BitwiseOperation.Or, "activity:scratch", "activity:20260727", "activity:20260728");

        Assert.Equal(8, result);
    }

    [Fact]
    public async Task DeleteAsync_CallsKeyDelete()
    {
        await _sut.DeleteAsync("activity:scratch");

        _database.Verify(d => d.KeyDeleteAsync("activity:scratch", It.IsAny<CommandFlags>()), Times.Once);
    }
}
