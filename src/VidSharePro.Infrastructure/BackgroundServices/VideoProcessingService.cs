using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using VidSharePro.Application.Common.Configuration;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Domain.Entities;

namespace VidSharePro.Infrastructure.BackgroundServices;

public class VideoProcessingService(
    IServiceProvider serviceProvider,
    IOptions<StorageOptions> storageOptions,
    IOptions<FFmpegOptions> ffmpegOptions,
    ILogger<VideoProcessingService> logger) : BackgroundService, IVideoProcessingService
{
    // Use the options to create the local variables the code is looking for
    private readonly string _storagePath = storageOptions.Value.Path;
    private readonly string _ffmpegPath = ffmpegOptions.Value.ExecutablePath;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The worker runs in a continuous loop until the application stops
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Create a scope to access the Database (Repositories are Scoped, Workers are Singleton)
            using (var scope = serviceProvider.CreateScope())
            {
                var videoRepository = scope.ServiceProvider.GetRequiredService<IVideoRepository>();

                // 2. Fetch videos with the 'PendingValidation' status
                var pendingVideos = await videoRepository.GetPendingVideosAsync(stoppingToken);

                foreach (var video in pendingVideos)
                {
                    try
                    {
                        // 3. Mark as Processing so other worker instances don't grab it
                        // Note: You might need to add 'MarkAsProcessing' or 'Processing' to your Enum 
                        // if you want to track this state specifically.

                        // 4. Define Paths
                        // 'video.OriginalFilePath' should be the property where you stored the raw upload
                        string inputPath = video.StoragePath;
                        string outputPath = inputPath.Replace(".tmp", ".mp4"); // Example rename
                        string thumbPath = inputPath.Replace(".tmp", ".jpg"); // The new thumbnail path

                        // 5. Execute FFmpeg (The method we discussed in the previous step)
                        bool success = await ProcessVideoWithFFmpeg(inputPath, outputPath);

                        if (success)
                        {
                            // STEP 2: Generate the Thumbnail (The new part)
                            // -ss 00:00:01 (Jump to 1 second mark)
                            // -vframes 1 (Take exactly one frame)
                            var thumbArgs = $"-i \"{outputPath}\" -ss 00:00:01 -vframes 1 \"{thumbPath}\"";

                            bool thumbSuccess = await RunFFmpegCommand(thumbArgs);

                            video.MarkAsReady(); // Updates Status to VideoStatus.Ready
                                                 // Update the video record to point to the new processed file
                                                 // video.UpdateStoragePath(outputPath); 
                        }
                        else
                        {
                            video.MarkAsFailed(""); // Updates Status to VideoStatus.Failed
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error (consider injecting ILogger)
                        logger.LogError(ex, "Actual error occurred during VideProcessingService ExecuteAsync");
                        video.MarkAsFailed("");
                    }

                    // 6. Save changes to the Database
                    await videoRepository.UpdateAsync(video, stoppingToken);
                }
            }

            // 7. Wait 10 seconds before polling the database again to save CPU
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task<bool> ProcessVideoWithFFmpeg(string inputPath, string outputPath)
    {
        // This command converts the video to a standard web-friendly H.264 MP4
        var arguments = $"-i \"{inputPath}\" -c:v libx264 -crf 23 -preset fast -c:a aac -b:a 128k \"{outputPath}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg", // Ensure ffmpeg is in your System PATH
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        process.Start();
        await process.WaitForExitAsync();

        return process.ExitCode == 0;
    }

    private async Task<bool> RunFFmpegCommand(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg", // Ensure this is in your System PATH
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync();

        return process.ExitCode == 0;
    }

    public async Task ProcessAndGenerateThumbnailAsync(Video video, CancellationToken ct)
    {
        // 1. Setup Paths
        var inputPath = Path.Combine(_storagePath, video.StoragePath)
            .Replace("/", Path.DirectorySeparatorChar.ToString());  // Ensure this is a full path or combined with _storagePath
        var thumbnailDir = Path.Combine(_storagePath, "thumbnails");
        var outputPath = Path.Combine(thumbnailDir, $"{video.Id}.jpg");

        // Ensure the input file actually exists before calling FFmpeg
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException($"FFmpeg Input missing: {inputPath}");
        }

        if (!Directory.Exists(thumbnailDir))
            Directory.CreateDirectory(thumbnailDir);

        // 2. FFmpeg Arguments: Grab 1 frame (-vframes 1) at the 1-second mark (-ss 00:00:01)
        var args = $"-i \"{inputPath}\" -ss 00:00:01 -vframes 1 -f image2 -y \"{outputPath}\"";

        logger.LogInformation("Running FFmpeg: {Path} {Args}", _ffmpegPath, args);

        var startInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            Arguments = args,
            RedirectStandardError = true, // FFmpeg sends logs to StandardError
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(ct);
                throw new Exception($"FFmpeg failed with exit code {process.ExitCode}: {error}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "FFmpeg process crashed.");
            throw;
        }
    }

}