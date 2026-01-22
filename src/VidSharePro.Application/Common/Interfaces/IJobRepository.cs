// VidSharePro.Application/Common/Interfaces/IJobRepository.cs
using VidSharePro.Domain.Entities;
using VidSharePro.Domain.Enums;

namespace VidSharePro.Application.Common.Interfaces;

public interface IJobRepository
{
    Task<BackgroundJob?> GetNextJobAsync(CancellationToken ct = default);
    Task AddAsync(BackgroundJob job, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid jobId, JobStatus status, string? error = null, CancellationToken ct = default);
}