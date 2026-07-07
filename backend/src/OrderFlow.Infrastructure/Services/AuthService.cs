using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository         _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IDateTimeProvider       _dateTime;
    private readonly IConfiguration          _config;

    public AuthService(
        IUserRepository         userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IDateTimeProvider       dateTime,
        IConfiguration          config)
    {
        _userRepository         = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _dateTime               = dateTime;
        _config                 = config;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.GetByUsernameAsync(request.Username, cancellationToken) is not null)
            return null;

        if (await _userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
            return null;

        var user = new User
        {
            Username     = request.Username,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = "Customer"
        };

        var created = await _userRepository.CreateAsync(user, cancellationToken);
        return await BuildAuthResponseAsync(created, cancellationToken);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return await BuildAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetActiveByTokenAsync(refreshToken, cancellationToken);
        if (token is null) return null;

        await _refreshTokenRepository.RevokeAsync(token, cancellationToken);
        return await BuildAuthResponseAsync(token.User!, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        => await _refreshTokenRepository.RevokeByTokenAsync(refreshToken, cancellationToken);

    // ─── private helpers ────────────────────────────────────────────────────

    private async Task<AuthResponse> BuildAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var (accessToken, expiresAt) = GenerateAccessToken(user);
        var expDays                  = int.TryParse(_config["JwtSettings:RefreshTokenExpirationDays"], out var d) ? d : 7;
        var newRefreshToken          = await _refreshTokenRepository.CreateAsync(
                                           user.Id, _dateTime.UtcNow.AddDays(expDays), cancellationToken);

        return new AuthResponse
        {
            UserId       = user.Id,
            Username     = user.Username,
            Email        = user.Email,
            Role         = user.Role,
            Token        = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt    = expiresAt
        };
    }

    private (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
    {
        var secretKey  = _config["JwtSettings:SecretKey"]!;
        var issuer     = _config["JwtSettings:Issuer"]    ?? "OrderFlow.API";
        var audience   = _config["JwtSettings:Audience"]  ?? "OrderFlow.Client";
        var expMinutes = int.TryParse(_config["JwtSettings:AccessTokenExpirationMinutes"], out var m) ? m : 15;

        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = _dateTime.UtcNow.AddMinutes(expMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email,      user.Email),
            new Claim(ClaimTypes.Role,                    user.Role),
            new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiry, signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }
}
