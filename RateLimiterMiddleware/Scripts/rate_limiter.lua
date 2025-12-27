-- KEYS[1]  -> rate limit key
-- ARGV[1]  -> capacity
-- ARGV[2]  -> refill rate (tokens per second)
-- ARGV[3]  -> current timestamp (epoch seconds)
-- ARGV[4]  -> ttl (seconds)

local key = KEYS[1]

local capacity = tonumber(ARGV[1])
local refill_rate = tonumber(ARGV[2])
local now = tonumber(ARGV[3])
local ttl = tonumber(ARGV[4])

-- Fetch existing state
local data = redis.call("HMGET", key, "tokens", "last_refill")

local tokens = tonumber(data[1])
local last_refill = tonumber(data[2])

-- Initialize bucket if not exists
if tokens == nil or last_refill == nil then
    tokens = capacity
    last_refill = now
end

-- Refill calculation
local elapsed = now - last_refill
if elapsed > 0 then
    local refill = elapsed * refill_rate
    tokens = math.min(capacity, tokens + refill)
    last_refill = now
end

-- Consume token if possible
local allowed = 0
if tokens >= 1 then
    tokens = tokens - 1
    allowed = 1
end

-- Persist updated state
redis.call("HSET", key,
    "tokens", tokens,
    "last_refill", last_refill)

-- Set TTL to clean inactive buckets
redis.call("EXPIRE", key, ttl)

return allowed
