// VidSharePro.Infrastructure/BackgroundJobs/VideoProcessingWorker.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Domain.Enums;

namespace VidSharePro.Infrastructure.BackgroundJobs;

public class VideoProcessingWorker(
    IServiceProvider serviceProvider,
    ILogger<VideoProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                var videoRepo = scope.ServiceProvider.GetRequiredService<IVideoRepository>();

                var job = await jobRepo.GetNextJobAsync(stoppingToken);
                if (job != null)
                {
                    try
                    {
                        await jobRepo.UpdateStatusAsync(job.Id, JobStatus.Processing);

                        // Logic: Here you would integrate FFmpeg for format validation/transcoding
                        // For now, we simulate validation
                        var videoId = Guid.Parse(job.ReferenceId);
                        var video = await videoRepo.GetByIdAsync(videoId);

                        if (video != null)
                        {
                            video.TransitionToReady();
                            await videoRepo.UpdateAsync(video);
                        }

                        await jobRepo.UpdateStatusAsync(job.Id, JobStatus.Completed);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Job {JobId} failed", job.Id);
                        await jobRepo.UpdateStatusAsync(job.Id, JobStatus.Failed, ex.Message);
                    }
                }
            }
            await Task.Delay(5000, stoppingToken); // Poll interval
        }
    }
}