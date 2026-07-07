using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<string> CreateAsync(int userId, DateTime expiresAt, CancellationToken cancellationToken = default);
    Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task RevokeByTokenAsync(string token, CancellationToken cancellationToken = default);
}
