using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/users/{userId:int}/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(FavoritesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FavoritesResponse>> GetFavorites(int userId)
    {
        var favorites = await _favoriteService.GetFavoritesAsync(userId);
        return Ok(favorites);
    }

    [HttpPost("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFavorite(int userId, int productId)
    {
        await _favoriteService.AddFavoriteAsync(userId, productId);
        return NoContent();
    }

    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveFavorite(int userId, int productId)
    {
        await _favoriteService.RemoveFavoriteAsync(userId, productId);
        return NoContent();
    }
}
