
public class TokenBucketRateLimiter : IRateLimiter
{
    private readonly ITokenBucketStore _store;
    private readonly int _capacity;
    private readonly int _windowTimeSeconds;

    public TokenBucketRateLimiter(
        ITokenBucketStore store,
        int capacity,
        int windowTimeSeconds
    ) {
        _store = store;
        _capacity = capacity;
        _windowTimeSeconds = windowTimeSeconds;
    }

    public Task<RateLimitResult> IsRequestAllowedAsync(string key)
    {
        var refillRate = _capacity / (double) _windowTimeSeconds;
        var bucket = _store.GetOrCreate(key, _capacity, refillRate);
        var allowed = bucket.TryConsume();
        
        return Task.FromResult(new RateLimitResult
        {
            IsAllowed = allowed,
            RemainingTokens = (int)bucket.dTokens
        });
    }
}