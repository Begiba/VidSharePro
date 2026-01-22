// VidSharePro.Application/Common/Interfaces/IVideoRepository.cs
using VidSharePro.Domain.Entities;

namespace VidSharePro.Application.Common.Interfaces;

public interface IVideoRepository
{
    Task<Video?> GetByIdAsync(Guid id, bool includeFormats = false, CancellationToken ct = default);
    Task<IEnumerable<Video>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task AddAsync(Video video, CancellationToken ct = default);
    Task<IEnumerable<Video>> GetPendingVideosAsync(CancellationToken ct);
    Task UpdateAsync(Video video, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<string?> GetThumbnailPathAsync(Guid id);
}