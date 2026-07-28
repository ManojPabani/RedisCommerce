using Moq;
using RedisCommerce.Infrastructure.Caching;
using StackExchange.Redis;
using Xunit;

namespace RedisCommerce.Tests.Infrastructure;

public class SortedSetServiceTests
{
    private readonly Mock<IDatabase> _database = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly SortedSetService _sut;

    public SortedSetServiceTests()
    {
        _connectionMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);

        _sut = new SortedSetService(_connectionMultiplexer.Object);
    }

    [Fact]
    public async Task AddAsync_CallsSortedSetAdd()
    {
        _database
            .Setup(d => d.SortedSetAddAsync("popular-products", "10", 5, It.IsAny<SortedSetWhen>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.AddAsync("popular-products", "10", 5);

        Assert.True(result);
    }

    [Fact]
    public async Task IncrementAsync_ReturnsNewScore()
    {
        _database
            .Setup(d => d.SortedSetIncrementAsync("popular-products", "10", 1, It.IsAny<CommandFlags>()))
            .ReturnsAsync(4);

        var result = await _sut.IncrementAsync("popular-products", "10", 1);

        Assert.Equal(4, result);
    }

    [Fact]
    public async Task RangeDescendingAsync_ReturnsScoredMembersHighestFirst()
    {
        _database
            .Setup(d => d.SortedSetRangeByRankWithScoresAsync("popular-products", 0, 1, Order.Descending, It.IsAny<CommandFlags>()))
            .ReturnsAsync([new SortedSetEntry("10", 5), new SortedSetEntry("50", 3)]);

        var result = await _sut.RangeDescendingAsync("popular-products", 0, 1);

        Assert.Equal(2, result.Count);
        Assert.Equal("10", result[0].Member);
        Assert.Equal(5, result[0].Score);
        Assert.Equal("50", result[1].Member);
        Assert.Equal(3, result[1].Score);
    }

    [Fact]
    public async Task ScoreAsync_MemberExists_ReturnsScore()
    {
        _database
            .Setup(d => d.SortedSetScoreAsync("popular-products", "10", It.IsAny<CommandFlags>()))
            .ReturnsAsync(5);

        var result = await _sut.ScoreAsync("popular-products", "10");

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task ScoreAsync_MemberMissing_ReturnsNull()
    {
        _database
            .Setup(d => d.SortedSetScoreAsync("popular-products", "10", It.IsAny<CommandFlags>()))
            .ReturnsAsync((double?)null);

        var result = await _sut.ScoreAsync("popular-products", "10");

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_CallsSortedSetRemove()
    {
        _database
            .Setup(d => d.SortedSetRemoveAsync("analytics:daily-active-counts", "20260601", It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.RemoveAsync("analytics:daily-active-counts", "20260601");

        Assert.True(result);
    }
}
