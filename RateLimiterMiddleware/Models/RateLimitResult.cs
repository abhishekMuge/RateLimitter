public struct RateLimitResult
{
    public bool IsAllowed {get; set;}
    public int RemainingTokens {get; set;}
}