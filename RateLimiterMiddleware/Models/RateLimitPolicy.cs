public sealed class RateLimitPolicy
{
    public int Capacity {get; set;}
    public double RefillRatePerSecond {get; set;}
}