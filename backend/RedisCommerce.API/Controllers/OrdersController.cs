using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CheckoutRequest> _checkoutValidator;

    public OrdersController(IOrderService orderService, IValidator<CheckoutRequest> checkoutValidator)
    {
        _orderService = orderService;
        _checkoutValidator = checkoutValidator;
    }

    [HttpPost("checkout")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> Checkout(CheckoutRequest request)
    {
        var validationResult = await _checkoutValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            AddValidationErrors(validationResult);
            return ValidationProblem(ModelState);
        }

        var order = await _orderService.CheckoutAsync(request);
        return StatusCode(StatusCodes.Status201Created, order);
    }

    private void AddValidationErrors(FluentValidation.Results.ValidationResult validationResult)
    {
        foreach (var error in validationResult.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
