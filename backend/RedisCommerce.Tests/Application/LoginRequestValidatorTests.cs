using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Validators;
using Xunit;

namespace RedisCommerce.Tests.Application;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    [Fact]
    public void Validate_PositiveUserId_IsValid()
    {
        var result = _sut.Validate(new LoginRequest(1001));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_NonPositiveUserId_IsInvalid(int userId)
    {
        var result = _sut.Validate(new LoginRequest(userId));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.UserId));
    }
}
