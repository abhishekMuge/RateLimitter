
using System.Text.Json;
using System.Text;
public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitIdentityResolver _identityResolver;
    private readonly IRateLimitPolicyResolver _policyResolver;
    private readonly ITokenBucketStore _bucketStore;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IRateLimitIdentityResolver identityResolver,
        IRateLimitPolicyResolver policyResolver,
        ITokenBucketStore bucketStore)
    {
        _next = next;
        _identityResolver = identityResolver;
        _policyResolver = policyResolver;
        _bucketStore = bucketStore;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Empty request body.");
            return;
        }

        RequestPacket packet;
        try
        {
            packet = JsonSerializer.Deserialize<RequestPacket>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
        }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid request packet.");
            return;
        }

        // 1. Resolve identity
        string identityKey;
        try
        {
            identityKey =
                _identityResolver.ResolveIdentity(packet.ValidatePacket);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(ex.Message);
            return;
        }

        // 2. Resolve policy
        var policy =
            _policyResolver.Resolver(packet.ValidatePacket);

        // 3. Get bucket
        var bucket =
            _bucketStore.GetOrCreate(
                identityKey,
                policy.Capacity,
                policy.RefillRatePerSecond);

        // 4. Enforce rate limit
        if (!bucket.TryConsume())
        {
            context.Response.StatusCode =
                StatusCodes.Status429TooManyRequests;

            await context.Response.WriteAsync(
                "Rate limit exceeded. Try again later.");

            return;
        }

        // 5. Forward request
        await _next(context);
    }
}
