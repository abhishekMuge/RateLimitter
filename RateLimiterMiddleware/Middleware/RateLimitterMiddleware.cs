
using System.Text.Json;
using System.Text;

public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiterService _rateLimiterService;
    public RateLimitingMiddleware(
        RequestDelegate next,
        IRateLimiterService rateLimiterService)
    {
        _next = next;
        _rateLimiterService = rateLimiterService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        // Only intercept JSON requests
        if (!context.Request.ContentType?.Contains("application/json") ?? true)
        {
            await _next(context);
            return;
        }

        context.Request.Body.Position = 0;
        var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            leaveOpen: true
        );
        var body = await reader.ReadToEndAsync();   
        context.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Empty request body");
            return;
        }

        RequestPacket packet;
        try
        {
            packet = JsonSerializer.Deserialize<RequestPacket>(body);
        }
        catch (JsonException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid JSON format");
            return;
        }

        var userId = packet?.ValidatePacket.UserId;
        var endpoint = packet?.ForwardPacket.EndPoint;

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(endpoint))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing rate limit identifiers");
            return;
        }

        var allowed = await _rateLimiterService.IsRequestAllowedAsync(
            userId,
            endpoint,
            maxRequests: 5,
            windowSeconds: 60
        );

        if(!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        try
        {
            // var forwardPayload = packet?.ForwardPacket.Payload ?? string.Empty;
            // var newBody = Encoding.UTF8.GetBytes(forwardPayload);

            // context.Request.Body = new MemoryStream(newBody);
            // context.Request.ContentLength = newBody.Length;

            // 5. Forward request
            await _next(context);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception Occured: {ex}");
        }
    }
}
