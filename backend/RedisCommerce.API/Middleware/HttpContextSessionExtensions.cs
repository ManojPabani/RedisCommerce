using RedisCommerce.Application.DTOs;

namespace RedisCommerce.API.Middleware;

public static class HttpContextSessionExtensions
{
    /// <summary>
    /// Returns the UserId of the session resolved by <see cref="SessionMiddleware"/> for this
    /// request, or null if no X-Session-Id header was sent or it didn't resolve to a live session.
    /// </summary>
    public static int? GetSessionUserId(this HttpContext context)
    {
        return context.Items[SessionMiddleware.SessionItemsKey] is SessionResponse session
            ? session.UserId
            : null;
    }
}
