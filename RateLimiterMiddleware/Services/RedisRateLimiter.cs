using StackExchange.Redis;
using System;
using System.IO;
using System.Threading.Tasks;

public class RedisRateLimiter : IRateLimiterService
{
    private readonly IDatabase _db;
    private readonly string _luaScript;
    private LoadedLuaScript _loadedScript;

    public RedisRateLimiter(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
        _luaScript = File.ReadAllText("Scripts/Fixed_Window_Limiter.lua");
        LoadScript();
    }

    private void LoadScript()
    {
        var server = _db.Multiplexer.GetServer(
            _db.Multiplexer.GetEndPoints()[0]);

        _loadedScript = LuaScript.Prepare(_luaScript)
                                  .Load(server);
    }

    public async Task<bool> IsRequestAllowedAsync(
        string userId,
        string endpoint,
        int maxRequests,
        int windowSeconds)
    {
        var cacheKey = $"rate_limit:{userId}:{endpoint}";

        try
        {
            // var evalParams = new {
            //     keys = new RedisKey[] { (RedisKey)key },
            //     argv = new RedisValue[] { maxRequests, windowSeconds }
            // };
            // var result = (int)await _loadedScript.EvaluateAsync(
            //     _db,
            //     new RedisKey[] { key },
            //     new RedisValue[]
            //     {
            //         maxRequests,
            //         windowSeconds,
            //         timestamp
            //     });

            var result = (int)await _loadedScript.EvaluateAsync(
                _db,
                new
                {
                    key = cacheKey,
                    limit = maxRequests,
                    window = windowSeconds
                }
            );
            // var redisResult = await _loadedScript.EvaluateAsync(_db, evalParams);
            // int result = (int)(long)redisResult;

            return result == 1;
        }
        catch (RedisServerException ex) when (ex.Message.Contains("NOSCRIPT"))
        {
            LoadScript();
            return await IsRequestAllowedAsync(
                userId, endpoint, maxRequests, windowSeconds);
        }
    }
}
