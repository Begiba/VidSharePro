namespace VidSharePro.API.Middleware;

public class QueryStringAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // If the request is for a video stream and has a token in the query string
        if (context.Request.Path.StartsWithSegments("/api/videos/stream") &&
            context.Request.Query.TryGetValue("token", out var token))
        {
            // Move the token to the Authorization Header so JWT Bearer can find it
            context.Request.Headers.Append("Authorization", $"Bearer {token}");
        }

        await next(context);
    }
}