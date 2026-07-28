using FluentValidation;
using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Validators;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);
    }
}
