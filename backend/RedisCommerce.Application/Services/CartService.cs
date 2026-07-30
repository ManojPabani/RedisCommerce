using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Caching;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Events;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Domain.Exceptions;

namespace RedisCommerce.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IRedisPublisher _publisher;
    private readonly ILogger<CartService> _logger;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IRedisPublisher publisher,
        ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<CartResponse> GetCartAsync(int userId)
    {
        var cart = await _cartRepository.GetCartAsync(userId);
        return ToResponse(userId, cart);
    }

    public async Task<CartResponse> AddItemAsync(int userId, AddCartItemRequest request)
    {
        _ = await _productRepository.GetByIdAsync(request.ProductId)
            ?? throw new ProductNotFoundException(request.ProductId);

        var currentQuantity = await _cartRepository.GetItemQuantityAsync(userId, request.ProductId) ?? 0;
        var newQuantity = currentQuantity + request.Quantity;

        await SetOrRemoveAsync(userId, request.ProductId, newQuantity);

        var cart = await GetCartAsync(userId);
        await PublishCartUpdatedAsync(userId, cart.Items.Count);
        return cart;
    }

    public async Task<CartResponse> UpdateItemAsync(int userId, int productId, UpdateCartItemRequest request)
    {
        await SetOrRemoveAsync(userId, productId, request.Quantity);

        var cart = await GetCartAsync(userId);
        await PublishCartUpdatedAsync(userId, cart.Items.Count);
        return cart;
    }

    public async Task<CartResponse> RemoveItemAsync(int userId, int productId)
    {
        await _cartRepository.RemoveItemAsync(userId, productId);
        _logger.LogInformation("Cart Item Removed: {Key} product {ProductId}", CacheKeys.Cart(userId), productId);

        var cart = await GetCartAsync(userId);
        await PublishCartUpdatedAsync(userId, cart.Items.Count);
        return cart;
    }

    public async Task ClearCartAsync(int userId)
    {
        await _cartRepository.ClearAsync(userId);
        _logger.LogInformation("Cart Cleared: {Key}", CacheKeys.Cart(userId));

        await PublishCartUpdatedAsync(userId, 0);
    }

    private async Task PublishCartUpdatedAsync(int userId, int itemCount)
    {
        await _publisher.PublishAsync(RedisChannels.Cart, new CartUpdatedEvent
        {
            Payload = new CartUpdatedPayload(userId, itemCount),
        });
    }

    private async Task SetOrRemoveAsync(int userId, int productId, int quantity)
    {
        if (quantity <= 0)
        {
            await _cartRepository.RemoveItemAsync(userId, productId);
            _logger.LogInformation("Cart Item Removed: {Key} product {ProductId}", CacheKeys.Cart(userId), productId);
            return;
        }

        await _cartRepository.SetItemQuantityAsync(userId, productId, quantity);
        _logger.LogInformation("Cart Updated: {Key} product {ProductId} quantity {Quantity}", CacheKeys.Cart(userId), productId, quantity);
    }

    private static CartResponse ToResponse(int userId, IDictionary<int, int> cart)
    {
        var items = cart
            .Select(kvp => new CartItemResponse(kvp.Key, kvp.Value))
            .ToList();

        return new CartResponse(userId, items);
    }
}
