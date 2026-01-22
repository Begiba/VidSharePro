using VidSharePro.Domain.Entities;

namespace VidSharePro.Application.Common.Interfaces;

public interface IVideoProcessingService
{
    Task ProcessAndGenerateThumbnailAsync(Video video, CancellationToken ct);
}