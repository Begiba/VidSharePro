using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Infrastructure.Persistence;
using VidSharePro.Infrastructure.Persistence.Repositories;
using VidSharePro.Infrastructure.Storage;
using VidSharePro.Infrastructure.Security;
using VidSharePro.Infrastructure.Monitoring;
using VidSharePro.Infrastructure.BackgroundJobs;

namespace VidSharePro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. SQL Server Persistence
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // 2. Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IShareRepository, ShareRepository>();
        services.AddScoped<IJobRepository, JobRepository>();

        // 3. Infrastructure Services
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<IAuthService, InfrastructureAuthService>();
        services.AddSingleton<IMetricsService, MetricsService>();

        // 4. Background Workers (IHostedService)
        services.AddHostedService<JobProcessorWorker>();
        services.AddHostedService<CleanupScheduler>();

        return services;
    }
}