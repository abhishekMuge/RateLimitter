public interface IRateLimitPolicyResolver
{
    RateLimitPolicy Resolver(ValidatePacket validatePacket);
}

