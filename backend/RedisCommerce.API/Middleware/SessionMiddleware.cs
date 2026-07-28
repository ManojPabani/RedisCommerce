using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.API.Middleware;

/// <summary>
/// Opportunistically resolves and slides the TTL of the session named by the
/// X-Session-Id header. Does not require or enforce a session — requests without
/// the header (i.e. every Phase 1/2 endpoint) behave exactly as before.
/// </summary>
public class SessionMiddleware
{
    public const string SessionHeaderName = "X-Session-Id";
    public const string SessionItemsKey = "Session";

    private readonly RequestDelegate _next;

    public SessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISessionService sessionService)
    {
        if (context.Request.Headers.TryGetValue(SessionHeaderName, out var sessionId) &&
            !string.IsNullOrWhiteSpace(sessionId))
        {
            var session = await sessionService.RefreshSessionAsync(sessionId!);
            context.Items[SessionItemsKey] = session;
        }

        await _next(context);
    }
}
