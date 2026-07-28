using Moq;
using RedisCommerce.Infrastructure.Caching;
using StackExchange.Redis;
using Xunit;

namespace RedisCommerce.Tests.Infrastructure;

public class RedisCacheServiceTests
{
    private readonly Mock<IDatabase> _database = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly RedisCacheService _sut;

    public RedisCacheServiceTests()
    {
        _connectionMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);

        _sut = new RedisCacheService(_connectionMultiplexer.Object);
    }

    [Fact]
    public async Task GetAsync_KeyExists_ReturnsStoredValue()
    {
        _database
            .Setup(d => d.StringGetAsync("product:1", It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)"cached-json");

        var result = await _sut.GetAsync("product:1");

        Assert.Equal("cached-json", result);
    }

    [Fact]
    public async Task GetAsync_KeyMissing_ReturnsNull()
    {
        _database
            .Setup(d => d.StringGetAsync("product:1", It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var result = await _sut.GetAsync("product:1");

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_CallsStringSetWithGivenExpiration()
    {
        var expiration = TimeSpan.FromMinutes(30);

        await _sut.SetAsync("product:1", "value", expiration);

        _database.Verify(d => d.StringSetAsync(
            "product:1",
            "value",
            new Expiration(expiration),
            It.IsAny<ValueCondition>(),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_CallsKeyDelete()
    {
        await _sut.RemoveAsync("product:1");

        _database.Verify(d => d.KeyDeleteAsync("product:1", It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task RefreshExpirationAsync_TouchesTtlWithoutRewritingValue()
    {
        var ttl = TimeSpan.FromMinutes(30);
        _database
            .Setup(d => d.KeyExpireAsync("session:abc", ttl, It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.RefreshExpirationAsync("session:abc", ttl);

        Assert.True(result);
        _database.Verify(d => d.StringSetAsync(
            It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<Expiration>(), It.IsAny<ValueCondition>(), It.IsAny<CommandFlags>()),
            Times.Never);
    }
}
