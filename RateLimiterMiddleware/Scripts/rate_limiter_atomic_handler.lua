local key = KEYS[1]
local limit = tonumber(ARGV[1])
local window = tonumber(ARGV[2])
local now = tonumber(ARGV[3])

local current = redis.call("GET", key)

if current then
    if tonumber(current) >= limit then
        return 0
    else
        redis.call("INCR", key)
        return 1
    end
else
    redis.call("SET", key, 1, "EX", window)
    return 1
end