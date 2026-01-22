// VidSharePro.API/Extensions/SecurityConfiguration.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace VidSharePro.API.Extensions;
// 1. MUST be a static class
public static class SecurityConfiguration
{
    // 2. MUST be a static method with 'this' keyword
    public static void ConfigureSecurity(this WebApplication app)
    {
        // Security Headers Middleware
        app.Use(async (context, next) =>
        {
            // SKIP SECURITY HEADERS FOR SWAGGER
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/favicon.ico"))
            {
                await next();
                return;
            }
            // CSP logic...
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self' https://code.jquery.com; " + // Allow jQuery CDN
                "style-src 'self' 'unsafe-inline'; " +          // Allow our UI styles
                "img-src 'self' data: blob:; " +                // Allow thumbnails
                "media-src 'self' blob:;" +                     // Allow video streaming                
                "connect-src 'self' http://localhost:* ws://localhost:* https://localhost:*;"); // Allow API and BrowserLink
            await next();
        });

        // Enable Rate Limiting
        app.UseRateLimiter();
    }
}