using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Text.Json;

public class RateLimitterMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
    private readonly int _capacity;
    private readonly int _timeWindowSeconds;

    public RateLimitterMiddleware(RequestDelegate next, int capacity = 10, int timeWindowSeconds = 60)
    {
        _next = next;
        _capacity = capacity;
        _timeWindowSeconds = timeWindowSeconds;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;
        if (string.IsNullOrWhiteSpace(body))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid Request Object");
            return;
        }
        try
        {
            var packet = JsonSerializer.Deserialize<RequestPacket>(body);
            if (packet?.ValidatePacket == null || string.IsNullOrEmpty(packet.ValidatePacket.UserId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Mising Validation Information");
                return;
            }
            var key = packet.ValidatePacket.UserId;
            var bucket = _buckets.GetOrAdd(
                key,
                _ => new TokenBucket(_capacity, _capacity / (double)_timeWindowSeconds)
            );
            if (!bucket.TryConsume())
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Rate Limit Exceed, Try Later");
                return;
            }
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Error: {ex.Message}");
        }
    }
}