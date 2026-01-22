// VidSharePro.Application/Services/VideoService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Application.DTOs;
using VidSharePro.Domain.Entities;
using VidSharePro.Domain.Enums;
using VidSharePro.Application.Common.Configuration;

namespace VidSharePro.Application.Services;

public class VideoService(
    IVideoRepository videoRepository,
    IUserRepository userRepository,
    IFileStorage fileStorage,
    IJobRepository jobRepository,
    ILogger<VideoService> logger,      // <--- Added logger dependency
    IMetricsService metrics,
    IOptions<StorageOptions> storageOptions)           // <--- Added metrics dependency
{
    public async Task<Guid> UploadVideoAsync(Guid userId, UploadVideoRequestDto request, CancellationToken ct)
    {
        logger.LogInformation("Starting upload for {FileName} by user {UserId}", request.FileName, userId);
        var sw = Stopwatch.StartNew();

        // 1. Validate User Domain Rule
        var user = await userRepository.GetByIdAsync(userId, ct)
                    ?? throw new KeyNotFoundException("User not found");
        user.EnsureCanUpload();

        // 2. Persist File to Infrastructure (This uses your existing IFileStorage service)
        // This is better than manual Path.Combine because it hides folders from the Application layer.
        var storagePath = await fileStorage.SaveFileAsync(request.FileStream, request.FileName, ct);

        // 3. Create Domain Entity 
        // Ensure your Video constructor in Domain/Entities/Video.cs matches these arguments
        var video = new Video(
            request.Title,
            request.FileName,
            request.FileSize,
            storagePath,
            userId
        );
        // 4. Handle Formats and Lifecycle
        // Keeping your logic for tracking different versions of the video
        video.AddFormat("Original", storagePath, request.ContentType, request.FileSize);
        video.CompleteUpload(); // This likely sets status to PendingValidation

        // 5. Save Metadata
        await videoRepository.AddAsync(video, ct);

        // 6. Queue Background Validation Job (For your Background Worker to pick up)
        var validationJob = new BackgroundJob("VideoValidation", video.Id.ToString());
        await jobRepository.AddAsync(validationJob, ct);

        metrics.IncrementUploadCount();
        logger.LogInformation("Upload completed in {Elapsed}ms. VideoId: {VideoId}", sw.ElapsedMilliseconds, video.Id);

        return video.Id;
    }

    public async Task<Stream> GetVideoStreamAsync(Guid videoId, CancellationToken ct)
    {
        var video = await videoRepository.GetByIdAsync(videoId, true, ct)
                    ?? throw new KeyNotFoundException("Video not found");

        if (video.Status != VideoStatus.Ready)
            throw new InvalidOperationException("Video is still being processed.");

        var originalFormat = video.Formats.FirstOrDefault(f => f.ResolutionLabel == "Original")
                             ?? throw new FileNotFoundException("Video file not found.");

        return await fileStorage.GetFileStreamAsync(originalFormat.StoragePath, ct);
    }

    public async Task<VideoDto?> GetVideoByIdAsync(Guid id, CancellationToken ct)
    {
        var video = await videoRepository.GetByIdAsync(id, true, ct);

        if (video == null) return null;

        // Manual Mapping (Entity to DTO)
        return new VideoDto
        {
            Id = video.Id,
            Title = video.Title,
            Status = video.Status.ToString(),
            FileSize = video.FileSizeInBytes,
            CreatedAt = video.CreatedAt
        };
    }

    public async Task SoftDeleteVideoAsync(Guid videoId, CancellationToken ct)
    {
        var video = await videoRepository.GetByIdAsync(videoId, false, ct)
                    ?? throw new KeyNotFoundException("Video not found");

        // Logic: In Clean Architecture, the Repository handles the flag 
        // or the Entity has a .Delete() method.
        await videoRepository.DeleteAsync(video.Id, ct);

        logger.LogInformation("Video {VideoId} was soft-deleted.", videoId);
    }

    // Add this to VideoService.cs for the Controller's use
    public async Task<string?> GetPhysicalPathAsync(Guid id, CancellationToken ct)
    {
        var video = await videoRepository.GetByIdAsync(id, true, ct);
        return video?.Formats.FirstOrDefault()?.StoragePath;
    }

    public async Task<IEnumerable<Video>> GetUserVideosAsync(Guid userId, CancellationToken ct)
    {
        // Use the specific name from your repository
        return await videoRepository.GetByOwnerIdAsync(userId, ct);
    }

    public async Task<string?> GetThumbnailPathAsync(Guid id)
    {
        var video = await videoRepository.GetByIdAsync(id);

        if (video == null) return null;

        // Assuming your Video entity has a ThumbnailPath property
        // Or you construct it based on the video's ID/StoragePath
        var thumbnailPath = Path.Combine(storageOptions.Value.Path, "thumbnails", $"{id}.jpg");

        return thumbnailPath;
    }
}