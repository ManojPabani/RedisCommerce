using FluentValidation;
using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
