using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext       _context;
    private readonly IDateTimeProvider  _dateTime;

    public RefreshTokenRepository(AppDbContext context, IDateTimeProvider dateTime)
    {
        _context  = context;
        _dateTime = dateTime;
    }

    public async Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(
                r => r.Token == token && !r.IsRevoked && r.ExpiresAt > _dateTime.UtcNow,
                cancellationToken);

    public async Task<string> CreateAsync(int userId, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            UserId    = userId,
            Token     = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = expiresAt,
            CreatedAt = _dateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
        return refreshToken.Token;
    }

    public async Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        token.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);

        if (entity is null) return;

        entity.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
