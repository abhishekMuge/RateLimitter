public sealed class StaticRateLimitPolicyResolver : IRateLimitPolicyResolver
{
    public RateLimitPolicy Resolver (ValidatePacket validatePacket)
    {
        return validatePacket.ApiKey switch
        {
            "User" => new RateLimitPolicy
            {
                Capacity = 50,
                RefillRatePerSecond = 50
            },
            "Test-User" => new RateLimitPolicy
            {
                Capacity = 5,
                RefillRatePerSecond = 100
            },

            _ => new RateLimitPolicy
            {
                Capacity = 2,
                RefillRatePerSecond = 2
            } 
        };
    }
}