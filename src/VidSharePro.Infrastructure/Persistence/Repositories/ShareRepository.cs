// ShareRepository.cs
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Infrastructure.Persistence;

public class ShareRepository(AppDbContext context) : IShareRepository
{
    // Missing member 1: GetByTokenAsync
    public async Task<Share?> GetByTokenAsync(string token, CancellationToken ct)
        => await context.Shares.FirstOrDefaultAsync(s => s.Token == token, ct);

    // Missing member 2: AddAsync
    public async Task AddAsync(Share share, CancellationToken ct)
    {
        await context.Shares.AddAsync(share, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteExpiredAsync(CancellationToken ct)
    {
        var expired = context.Shares.Where(s => s.ExpiryDate < DateTime.UtcNow);
        context.Shares.RemoveRange(expired);
        await context.SaveChangesAsync(ct);
    }
}