// VidSharePro.API/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using VidSharePro.API.Extensions;
using VidSharePro.API.Middleware;
using VidSharePro.Application; // Namespace from step 1
using VidSharePro.Application.Common.Configuration;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Infrastructure;
using VidSharePro.Infrastructure.BackgroundServices;
using VidSharePro.Infrastructure.Monitoring;
using VidSharePro.Infrastructure.Persistence; // Namespace from step 2

var builder = WebApplication.CreateBuilder(args);

// --- The Wiring ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IMetricsService, MetricsService>();
// ------------------
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Increase the limit for multipart form data (the actual file upload)
builder.Services.Configure<FormOptions>(options =>
{
    // Set to 2GB
    options.MultipartBodyLengthLimit = 2147483648; //2GB
});

// 1. Serilog Setup
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
builder.WebHost.ConfigureKestrel(options =>
{
    // Set max request body size to 2GB (2 * 1024 * 1024 * 1024)
    //options.Limits.MaxRequestBodySize = 2147483648;
    options.Limits.MaxRequestBodySize = null;
});
// 2. Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ActiveUser", policy => policy.RequireClaim("IsActive", "True"));
});

// 3. Infrastructure & Application DI (Extension methods recommended)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
// Add to builder.Services in Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Policy for Video Streaming: 100 requests per minute per IP
    options.AddFixedWindowLimiter("streaming", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 20;
    });

    // Policy for Video Uploads: 5 uploads per hour per User
    options.AddFixedWindowLimiter("uploading", opt =>
    {
        opt.Window = TimeSpan.FromHours(1);
        opt.PermitLimit = 5;
    });
});
builder.Services.AddHostedService<VideoProcessingService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This converts Enums (0, 1, 2) to Strings ("Pending", "Ready")
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddHealthChecks();
// In a real Prometheus scenario, use: builder.Services.AddOpenTelemetry().WithMetrics(...)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IVideoProcessingService, VideoProcessingService>();
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<FFmpegOptions>(builder.Configuration.GetSection("FFmpeg"));

var app = builder.Build();

app.ConfigureSecurity();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VidSharePro API V1");
        c.RoutePrefix = "swagger"; // Access via http://localhost:5020/swagger
    });
}

// 4. Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>(); // Custom global handler
//app.UseHttpsRedirection();
app.UseDefaultFiles(); // Automatically looks for index.html
app.UseStaticFiles(); // For jQuery frontend

app.UseRouting();
app.UseMiddleware<QueryStringAuthMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        Log.Information("Applying migrations...");
        await context.Database.MigrateAsync();
        await VidSharePro.Infrastructure.Persistence.DbInitializer.SeedAsync(context);
        Log.Information("Database is ready.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred during database migration/seeding.");
    }
}

Log.Information("Starting web host...");
app.Use(async (context, next) =>
{
    context.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
    await next();
});
app.Run();