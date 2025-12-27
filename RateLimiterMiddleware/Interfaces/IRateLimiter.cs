public interface IRateLimiter
{
    Task <RateLimitResult> IsRequestAllowedAsync(string key);
}

public interface IRateLimiterService
{
    public Task<bool> IsRequestAllowedAsync(
        string userId,
        string endpoint,
        int maxRequests,
        int windowSeconds
    );
}