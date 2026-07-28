using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RedisCommerce.API.Extensions;
using RedisCommerce.API.Middleware;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(ISessionService sessionService, IValidator<LoginRequest> loginValidator)
    {
        _sessionService = sessionService;
        _loginValidator = loginValidator;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessionResponse>> Login(LoginRequest request)
    {
        if (!this.ApplyValidationErrors(await _loginValidator.ValidateAsync(request)))
        {
            return ValidationProblem(ModelState);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var session = await _sessionService.LoginAsync(request, ipAddress, userAgent);
        return Ok(session);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        if (Request.Headers.TryGetValue(SessionMiddleware.SessionHeaderName, out var sessionId) &&
            !string.IsNullOrWhiteSpace(sessionId))
        {
            await _sessionService.LogoutAsync(sessionId!);
        }

        return NoContent();
    }

    [HttpGet("session")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionResponse>> GetSession()
    {
        if (!Request.Headers.TryGetValue(SessionMiddleware.SessionHeaderName, out var sessionId) ||
            string.IsNullOrWhiteSpace(sessionId))
        {
            return NotFound();
        }

        var session = await _sessionService.GetSessionAsync(sessionId!);
        return session is null ? NotFound() : Ok(session);
    }
}
