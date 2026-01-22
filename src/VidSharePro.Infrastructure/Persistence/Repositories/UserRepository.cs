// UserRepository.cs
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Domain.Entities;
using VidSharePro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
        => await context.Users.FindAsync(new object[] { id }, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
        => await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    // Missing member 1: GetByUsernameAsync
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
        => await context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);
    }
    // Missing member 2: UpdateAsync
    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(ct);
    }
}

