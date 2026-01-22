// VidSharePro.Infrastructure/BackgroundJobs/CleanupScheduler.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Domain.Entities;

public class CleanupScheduler(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Run cleanup once every 24 hours
            using (var scope = serviceProvider.CreateScope())
            {
                var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                await jobRepo.AddAsync(new BackgroundJob("CleanupExpiredShares", "SYSTEM"), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}