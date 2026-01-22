// VidSharePro.Infrastructure/Persistence/Repositories/JobRepository.cs
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Domain.Entities;
using VidSharePro.Domain.Enums;
using VidSharePro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
public class JobRepository(AppDbContext context) : IJobRepository
{
    public async Task<BackgroundJob?> GetNextJobAsync(CancellationToken ct)
    {
        return await context.Jobs
            .Where(j => j.Status == JobStatus.Queued)
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task UpdateStatusAsync(Guid jobId, JobStatus status, string? error = null, CancellationToken ct = default)
    {
        var job = await context.Jobs.FindAsync(new object[] { jobId }, ct);
        if (job != null)
        {
            job.UpdateStatus(status);
            if (error != null) job.MarkAsFailed(error); // Assuming MarkAsFailed added to Domain
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task AddAsync(BackgroundJob job, CancellationToken ct)
    {
        await context.Jobs.AddAsync(job, ct);
        await context.SaveChangesAsync(ct);
    }
}