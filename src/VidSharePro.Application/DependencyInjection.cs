using Microsoft.Extensions.DependencyInjection;
using VidSharePro.Application.Services;

namespace VidSharePro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all Application Services
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<VideoService>();
        services.AddScoped<ShareService>();

        return services;
    }
}