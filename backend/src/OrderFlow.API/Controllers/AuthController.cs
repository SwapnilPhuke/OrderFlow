using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;

namespace OrderFlow.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService              _authService;
    private readonly ILogger<AuthController>   _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    /// <summary>Register a new customer account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(request, cancellationToken);
        if (result is null)
            return BadRequest(new { message = "Username or email is already registered." });

        _logger.LogInformation("New user registered: {Username}", request.Username);
        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>Authenticate and receive JWT + refresh token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (result is null)
            return Unauthorized(new { message = "Invalid username or password." });

        _logger.LogInformation("User logged in: {Username}", request.Username);
        return Ok(result);
    }

    /// <summary>Rotate refresh token — returns a new access token + refresh token pair.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (result is null)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        return Ok(result);
    }

    /// <summary>Revoke the current refresh token (logout).</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }
}
