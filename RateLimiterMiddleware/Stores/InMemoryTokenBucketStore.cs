using System.Collections.Concurrent;

public class InMemoryTokenBucketStore : ITokenBucketStore
{
    private readonly ConcurrentDictionary<string, TokenBucket> _bucket = new();

    public TokenBucket GetOrCreate(
        string key,
        int capacity, 
        double refilrate
    )
    {
        return _bucket.GetOrAdd(
            key,
            _ => new TokenBucket(capacity, refilrate)
        );
    }
}