using Moq;
using RedisCommerce.Infrastructure.Caching;
using StackExchange.Redis;
using Xunit;

namespace RedisCommerce.Tests.Infrastructure;

public class SetServiceTests
{
    private readonly Mock<IDatabase> _database = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly SetService _sut;

    public SetServiceTests()
    {
        _connectionMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);

        _sut = new SetService(_connectionMultiplexer.Object);
    }

    [Fact]
    public async Task AddAsync_NewMember_ReturnsTrue()
    {
        _database
            .Setup(d => d.SetAddAsync("favorites:1001", "10", It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.AddAsync("favorites:1001", "10");

        Assert.True(result);
    }

    [Fact]
    public async Task AddAsync_DuplicateMember_ReturnsFalse()
    {
        _database
            .Setup(d => d.SetAddAsync("favorites:1001", "10", It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        var result = await _sut.AddAsync("favorites:1001", "10");

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveAsync_CallsSetRemove()
    {
        _database
            .Setup(d => d.SetRemoveAsync("favorites:1001", "10", It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.RemoveAsync("favorites:1001", "10");

        Assert.True(result);
    }

    [Fact]
    public async Task MembersAsync_ReturnsAllMembers()
    {
        _database
            .Setup(d => d.SetMembersAsync("favorites:1001", It.IsAny<CommandFlags>()))
            .ReturnsAsync([(RedisValue)"10", (RedisValue)"50"]);

        var result = await _sut.MembersAsync("favorites:1001");

        Assert.Equal(["10", "50"], result);
    }

    [Fact]
    public async Task IsMemberAsync_CallsSetContains()
    {
        _database
            .Setup(d => d.SetContainsAsync("favorites:1001", "10", It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.IsMemberAsync("favorites:1001", "10");

        Assert.True(result);
    }

    [Fact]
    public async Task CardinalityAsync_ReturnsSetSize()
    {
        _database
            .Setup(d => d.SetLengthAsync("favorites:1001", It.IsAny<CommandFlags>()))
            .ReturnsAsync(2);

        var result = await _sut.CardinalityAsync("favorites:1001");

        Assert.Equal(2, result);
    }
}
