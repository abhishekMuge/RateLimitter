local key = @key
local limit = tonumber(@limit)
local window = tonumber(@window)

local current = redis.call("GET", key)

if current then
    if tonumber(current) >= limit then
        return 0
    end
    redis.call("INCR", key)
    return 1
else
    redis.call("SET", key, "1", "EX", window)
    return 1
end