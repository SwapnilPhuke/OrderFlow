using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Users.FindAsync(new object[] { id }, cancellationToken);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default)
        => await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);
}
