public interface IRateLimitIdentityResolver
{
    string ResolveIdentity(ValidatePacket validatePacket);
}
public sealed class CompositeIdentityResolver : IRateLimitIdentityResolver
{
    public string ResolveIdentity(ValidatePacket validatePacket)
    {
        if (string.IsNullOrWhiteSpace(validatePacket.ApiKey))
        {
            throw new InvalidOperationException("API Key is required for rate limiting");
        }
        if (string.IsNullOrWhiteSpace(validatePacket.UserId))
        {
            throw new InvalidOperationException("UserId required for the rate limiting");
        }
        return $"{validatePacket.ApiKey}:{validatePacket.UserId}";
    }
}