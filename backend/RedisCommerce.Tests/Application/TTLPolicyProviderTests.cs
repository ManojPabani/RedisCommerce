using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RedisCommerce.Application.Configuration;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Services;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class TTLPolicyProviderTests
{
    private readonly TTLPolicyProvider _sut;

    public TTLPolicyProviderTests()
    {
        var options = Options.Create(new RedisOptions
        {
            ProductTTLMinutes = 30,
            CartTTLHours = 24,
            SessionTTLMinutes = 30,
            AnalyticsTTLDays = 90,
        });

        _sut = new TTLPolicyProvider(options, Mock.Of<ILogger<TTLPolicyProvider>>());
    }

    [Theory]
    [InlineData(RedisObjectType.Product, 30 * 60)]
    [InlineData(RedisObjectType.Cart, 24 * 60 * 60)]
    [InlineData(RedisObjectType.Session, 30 * 60)]
    [InlineData(RedisObjectType.Analytics, 90 * 24 * 60 * 60)]
    public void GetTtl_ReturnsConfiguredDuration(RedisObjectType type, int expectedSeconds)
    {
        var result = _sut.GetTtl(type);

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), result);
    }

    [Theory]
    [InlineData(RedisObjectType.Popularity)]
    [InlineData(RedisObjectType.Favorites)]
    public void GetTtl_NeverExpiringTypes_ReturnsNull(RedisObjectType type)
    {
        var result = _sut.GetTtl(type);

        Assert.Null(result);
    }
}
