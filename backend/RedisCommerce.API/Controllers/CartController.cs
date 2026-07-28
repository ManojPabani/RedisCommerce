using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RedisCommerce.API.Extensions;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly IValidator<AddCartItemRequest> _addValidator;
    private readonly IValidator<UpdateCartItemRequest> _updateValidator;

    public CartController(
        ICartService cartService,
        IValidator<AddCartItemRequest> addValidator,
        IValidator<UpdateCartItemRequest> updateValidator)
    {
        _cartService = cartService;
        _addValidator = addValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{userId:int}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartResponse>> GetCart(int userId)
    {
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    [HttpPost("{userId:int}/items")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> AddItem(int userId, AddCartItemRequest request)
    {
        if (!this.ApplyValidationErrors(await _addValidator.ValidateAsync(request)))
        {
            return ValidationProblem(ModelState);
        }

        var cart = await _cartService.AddItemAsync(userId, request);
        return Ok(cart);
    }

    [HttpPut("{userId:int}/items/{productId:int}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartResponse>> UpdateItem(int userId, int productId, UpdateCartItemRequest request)
    {
        if (!this.ApplyValidationErrors(await _updateValidator.ValidateAsync(request)))
        {
            return ValidationProblem(ModelState);
        }

        var cart = await _cartService.UpdateItemAsync(userId, productId, request);
        return Ok(cart);
    }

    [HttpDelete("{userId:int}/items/{productId:int}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartResponse>> RemoveItem(int userId, int productId)
    {
        var cart = await _cartService.RemoveItemAsync(userId, productId);
        return Ok(cart);
    }

    [HttpDelete("{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearCart(int userId)
    {
        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }
}
