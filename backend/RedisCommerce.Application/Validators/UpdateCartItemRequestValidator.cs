using FluentValidation;
using RedisCommerce.Application.DTOs;

namespace RedisCommerce.Application.Validators;

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0);
    }
}
