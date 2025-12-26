public sealed class TokenBucket
{
    public int nCapacity { get; set; }
    public double dRefilRate { get; set; }
    public double dTokens { get; set; }
    public DateTime nLastRefil { get; set; }

    private readonly object _lock = new();

    public TokenBucket(int capacity, double refilRate)
    {
        nCapacity = capacity;
        dTokens = capacity;
        dRefilRate = refilRate;
        nLastRefil = DateTime.UtcNow;
    }
    /// <summary>
    /// Attempts to consume one token.
    /// Returns true if allowed, false if rate-limited.
    /// Thread-safe and atomic.
    /// </summary>
    public bool TryConsume()
    {
        lock(_lock)
        {
            // Refill();
            RefillIfNeeded();

            if (dTokens > 1)
            {
                dTokens -= 1;
                return true;
            }

            return false;   
        }
    }

    private void Refill()
    {
        var now = DateTime.UtcNow;
        var elapsedSeconds = (now - nLastRefil).Seconds;
        dTokens = Math.Min(nCapacity, dTokens + elapsedSeconds * dRefilRate);
        nLastRefil = now;
    }

    private void RefillIfNeeded()
    {
        var now = DateTime.UtcNow;
        var elapsedSeconds = (now - nLastRefil).Seconds;

        if(elapsedSeconds <= 0) return;
        var tokensToAdd = elapsedSeconds * dRefilRate;
        if(tokensToAdd > 0)
        {
            dTokens = Math.Min(nCapacity, dTokens + tokensToAdd);
        }
        nLastRefil = now;
    }
}