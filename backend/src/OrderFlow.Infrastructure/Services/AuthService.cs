using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext    _context;
    private readonly IConfiguration  _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config  = config;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower(), cancellationToken))
            return null;

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken))
            return null;

        var user = new User
        {
            Username     = request.Username,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = "Customer"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return await GenerateAuthResponse(user, cancellationToken);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower(), cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return await GenerateAuthResponse(user, cancellationToken);
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (token == null)
            return null;

        token.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);

        return await GenerateAuthResponse(token.User!, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (token != null)
        {
            token.IsRevoked = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // ─── private helpers ────────────────────────────────────────────────────

    private async Task<AuthResponse> GenerateAuthResponse(User user, CancellationToken cancellationToken)
    {
        var (accessToken, expiresAt) = GenerateAccessToken(user);
        var refreshToken             = await CreateRefreshTokenAsync(user.Id, cancellationToken);

        return new AuthResponse
        {
            UserId       = user.Id,
            Username     = user.Username,
            Email        = user.Email,
            Role         = user.Role,
            Token        = accessToken,
            RefreshToken = refreshToken,
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
        var expiry = DateTime.UtcNow.AddMinutes(expMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiry, signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    private async Task<string> CreateRefreshTokenAsync(int userId, CancellationToken cancellationToken)
    {
        var expDays = int.TryParse(_config["JwtSettings:RefreshTokenExpirationDays"], out var d) ? d : 7;

        var refreshToken = new RefreshToken
        {
            UserId    = userId,
            Token     = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(expDays)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
        return refreshToken.Token;
    }
}
