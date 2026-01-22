// VidSharePro.Infrastructure/Persistence/Repositories/VideoRepository.cs
using Microsoft.EntityFrameworkCore;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Domain.Entities;
using VidSharePro.Domain.Enums;


namespace VidSharePro.Infrastructure.Persistence.Repositories;

public class VideoRepository(AppDbContext context) : IVideoRepository
{
    public async Task<Video?> GetByIdAsync(Guid id, bool includeFormats = false, CancellationToken ct = default)
    {
        var query = context.Videos.AsQueryable();
        if (includeFormats) query = query.Include(v => v.Formats);

        return await query.FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task AddAsync(Video video, CancellationToken ct = default)
    {
        await context.Videos.AddAsync(video, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Video video, CancellationToken ct = default)
    {
        context.Videos.Update(video);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var video = await context.Videos.FindAsync(new object[] { id }, ct);
        if (video != null)
        {
            context.Videos.Remove(video); // Trigger Soft Delete logic in DbContext
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<IEnumerable<Video>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await context.Videos
            .AsNoTracking() // Optimization for read-only lists
            .Where(v => v.OwnerId == ownerId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Video>> GetPendingVideosAsync(CancellationToken ct)
    {
        return await context.Videos
            .Where(v => v.Status == VideoStatus.PendingValidation)
            .ToListAsync(ct);
    }

    public async Task<string?> GetThumbnailPathAsync(Guid id)
    {
        return await context.Videos
            .Where(v => v.Id == id)
            .Select(v => v.ThumbnailPath)
            .FirstOrDefaultAsync();
    }
}