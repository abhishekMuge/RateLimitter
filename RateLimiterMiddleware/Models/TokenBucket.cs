public class TokenBucket
{
    public int nCapacity { get; set; }
    public double dRefilRate { get; set; }
    public double dTokens { get; set; }
    public DateTime nLastRefil { get; set; }

    public TokenBucket(int capacity, double refilRate)
    {
        nCapacity = capacity;
        dTokens = capacity;
        dRefilRate = refilRate;
        nLastRefil = DateTime.UtcNow;
    }

    public bool TryConsume()
    {
        var now = DateTime.UtcNow;
        var elapsedSeconds = (now - nLastRefil).Seconds;
        dTokens = Math.Min(nCapacity, dTokens + elapsedSeconds * dRefilRate);
        nLastRefil = now;

        if (dTokens > 1)
        {
            dTokens -= 1;
            return true;
        }

        return false;
    }
}