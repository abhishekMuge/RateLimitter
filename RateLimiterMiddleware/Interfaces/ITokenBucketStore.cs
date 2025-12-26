
public interface ITokenBucketStore
{
    TokenBucket GetOrCreate(
        string key,
        int capacity,
        double refilrate
    );
}