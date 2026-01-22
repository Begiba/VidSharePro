// VidSharePro.Infrastructure/BackgroundJobs/JobProcessorWorker.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Domain.Entities;
using VidSharePro.Domain.Enums;
using VidSharePro.Infrastructure.BackgroundServices;

public class JobProcessorWorker(
    IServiceProvider serviceProvider,
    ILogger<JobProcessorWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("VidSharePro Job Processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();

                var job = await jobRepo.GetNextJobAsync(stoppingToken);
                if (job == null)
                {
                    await Task.Delay(10000, stoppingToken);
                    continue;
                }

                await ProcessJobInternal(job, scope.ServiceProvider, stoppingToken);
            }
            // This is the critical change:
            catch (OperationCanceledException)
            {
                // Do nothing. This is the system telling the worker to stop.
                logger.LogInformation("VidSharePro Job Processor is shutting down gracefully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Actual error occurred during job polling.");
                // Only delay if the token isn't cancelled
                if (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
    }

    protected async Task ProcessJobInternal(BackgroundJob job, IServiceProvider sp, CancellationToken ct)
    {
        var jobRepo = sp.GetRequiredService<IJobRepository>();
        await jobRepo.UpdateStatusAsync(job.Id, JobStatus.Processing, null, ct);

        try
        {
            switch (job.JobType)
            {
                case "VideoValidation":
                    await HandleVideoValidation(job, sp, ct);
                    break;
                case "CleanupExpiredShares":
                    await HandleShareCleanup(sp, ct);
                    break;
                default:
                    throw new NotSupportedException($"Job type {job.JobType} not supported.");
            }

            await jobRepo.UpdateStatusAsync(job.Id, JobStatus.Completed, null, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Job {JobId} failed: {Message}", job.Id, ex.Message);
            await jobRepo.UpdateStatusAsync(job.Id, JobStatus.Failed, ex.Message, ct);
        }
    }

    protected async Task HandleVideoValidation(BackgroundJob job, IServiceProvider sp, CancellationToken ct)
    {
        var videoRepo = sp.GetRequiredService<IVideoRepository>();
        var processingService = sp.GetRequiredService<IVideoProcessingService>();

        var video = await videoRepo.GetByIdAsync(Guid.Parse(job.ReferenceId), true, ct);
        if (video == null) return;

        // FIX: If the video is already Ready, just finish the job silently.
        if (video.Status == VideoStatus.Ready)
        {
            logger.LogInformation("Video {VideoId} is already Ready. Skipping processing.", video.Id);
            return;
        }

        try
        {
            video.StartProcessing();
            await videoRepo.UpdateAsync(video, ct);

            await processingService.ProcessAndGenerateThumbnailAsync(video, ct);

            video.TransitionToReady();
            await videoRepo.UpdateAsync(video, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Video processing failed for {VideoId}", video.Id);
            video.MarkAsFailed(ex.Message);
            await videoRepo.UpdateAsync(video, ct);
            throw;
        }
    }

    protected async Task HandleShareCleanup(IServiceProvider sp, CancellationToken ct)
    {
        var shareRepo = sp.GetRequiredService<IShareRepository>();
        await shareRepo.DeleteExpiredAsync(ct);
        logger.LogInformation("Expired shares cleaned up successfully.");
    }
}