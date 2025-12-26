public interface IRateLimiter
{
    Task <RateLimitResult> IsRequestAllowedAsync(string key);
}