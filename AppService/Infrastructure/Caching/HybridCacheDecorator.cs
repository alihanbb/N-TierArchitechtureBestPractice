using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Text.Json;

namespace AppService.Infrastructure.Caching;

/// <summary>
/// Hybrid Cache Decorator - Implements Decorator Pattern
/// Wraps both L1 (Memory) and L2 (Redis) cache implementations
/// Provides two-layer caching strategy for optimal performance
/// </summary>
public class HybridCacheDecorator : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<HybridCacheDecorator> _logger;
    private readonly IDatabase? _redisDb;
    private readonly bool _isRedisAvailable;

    // Cache TTL Configuration
    private readonly TimeSpan _l1DefaultTtl = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _l2DefaultTtl = TimeSpan.FromMinutes(30);
    private readonly TimeSpan _l1SlidingWindow = TimeSpan.FromMinutes(2);

    public HybridCacheDecorator(
        IMemoryCache memoryCache,
        IConnectionMultiplexer? redis,
        ILogger<HybridCacheDecorator> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _redis = redis;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _isRedisAvailable = TryInitializeRedis(out _redisDb);
        LogInitializationStatus();
    }

    private bool TryInitializeRedis(out IDatabase? database)
    {
        database = null;
        try
        {
            if (_redis?.IsConnected == true)
            {
                database = _redis.GetDatabase();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Redis connection");
            return false;
        }
    }

    private void LogInitializationStatus()
    {
        if (_isRedisAvailable)
        {
            _logger.LogInformation("✓ Hybrid Cache initialized: L1 (Memory) + L2 (Redis)");
        }
        else
        {
            _logger.LogWarning("⚠ Hybrid Cache initialized: L1 (Memory) only - Redis unavailable");
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // L1: Try Memory Cache first (fastest path)
            if (TryGetFromL1(key, out T? cachedValue))
            {
                return cachedValue;
            }

            // L2: Try Redis Cache (if available)
            if (_isRedisAvailable)
            {
                var redisValue = await TryGetFromL2<T>(key, cancellationToken);
                if (redisValue != null)
                {
                    // Promote to L1 for faster subsequent access
                    PromoteToL1(key, redisValue);
                    return redisValue;
                }
            }

            // Cache miss at both levels
            _logger.LogDebug("Cache MISS (L1 + L2) for key: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key: {Key}", key);
            return default;
        }
    }

    private bool TryGetFromL1<T>(string key, out T? value)
    {
        if (_memoryCache.TryGetValue(key, out value))
        {
            _logger.LogDebug("✓ L1 Cache HIT for key: {Key}", key);
            return true;
        }

        _logger.LogDebug("✗ L1 Cache MISS for key: {Key}", key);
        return false;
    }

    private async Task<T?> TryGetFromL2<T>(string key, CancellationToken cancellationToken)
    {
        if (_redisDb == null) return default;

        var redisValue = await _redisDb.StringGetAsync(key);
        if (!redisValue.IsNullOrEmpty)
        {
            _logger.LogDebug("✓ L2 Cache HIT for key: {Key} (promoting to L1)", key);
            return JsonSerializer.Deserialize<T>(redisValue!);
        }

        _logger.LogDebug("✗ L2 Cache MISS for key: {Key}", key);
        return default;
    }

    private void PromoteToL1<T>(string key, T value)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _l1DefaultTtl,
            SlidingExpiration = _l1SlidingWindow,
            Priority = CacheItemPriority.Normal
        };

        _memoryCache.Set(key, value, options);
        _logger.LogDebug("Promoted to L1: {Key}", key);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var l1Ttl = expiration ?? _l1DefaultTtl;
            var l2Ttl = expiration ?? _l2DefaultTtl;

            // Write to L1 (Memory Cache)
            SetInL1(key, value, l1Ttl);

            // Write to L2 (Redis Cache) if available
            if (_isRedisAvailable && _redisDb != null)
            {
                await SetInL2(key, value, l2Ttl, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key: {Key}", key);
        }
    }

    private void SetInL1<T>(string key, T value, TimeSpan ttl)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl,
            SlidingExpiration = TimeSpan.FromMinutes(Math.Min(ttl.TotalMinutes / 2, 5)),
            Priority = CacheItemPriority.Normal
        };

        _memoryCache.Set(key, value, options);
        _logger.LogDebug("L1 Cache SET: {Key} (TTL: {TTL})", key, ttl);
    }

    private async Task SetInL2<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        if (_redisDb == null) return;

        var json = JsonSerializer.Serialize(value);
        await _redisDb.StringSetAsync(key, json, ttl);
        _logger.LogDebug("L2 Cache SET: {Key} (TTL: {TTL})", key, ttl);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Remove from both layers
            RemoveFromL1(key);

            if (_isRedisAvailable && _redisDb != null)
            {
                await RemoveFromL2(key, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key: {Key}", key);
        }
    }

    private void RemoveFromL1(string key)
    {
        _memoryCache.Remove(key);
        _logger.LogDebug("L1 Cache REMOVE: {Key}", key);
    }

    private async Task RemoveFromL2(string key, CancellationToken cancellationToken)
    {
        if (_redisDb == null) return;

        await _redisDb.KeyDeleteAsync(key);
        _logger.LogDebug("L2 Cache REMOVE: {Key}", key);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("RemoveByPrefix: L1 doesn't support pattern matching, only L2 will be cleared");

            if (_isRedisAvailable && _redis != null && _redisDb != null)
            {
                await RemoveL2ByPattern(prefix, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing keys with prefix: {Prefix}", prefix);
        }
    }

    private async Task RemoveL2ByPattern(string prefix, CancellationToken cancellationToken)
    {
        if (_redis == null || _redisDb == null) return;

        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());

        var keys = server.Keys(pattern: $"{prefix}*").ToArray();
        if (keys.Any())
        {
            await _redisDb.KeyDeleteAsync(keys);
            _logger.LogInformation("L2 Cache removed {Count} keys with prefix: {Prefix}", keys.Length, prefix);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check L1 first
            if (_memoryCache.TryGetValue(key, out _))
            {
                return true;
            }

            // Check L2 if available
            if (_isRedisAvailable && _redisDb != null)
            {
                return await _redisDb.KeyExistsAsync(key);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking key existence: {Key}", key);
            return false;
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            // L1 is always healthy (in-memory)
            var l1Healthy = true;

            // Check L2 health if configured
            if (_isRedisAvailable && _redisDb != null)
            {
                await _redisDb.PingAsync();
            }

            return l1Healthy;
        }
        catch (Exception ex)
        {
            // Redis down, but L1 still works
            _logger.LogWarning(ex, "L2 (Redis) health check failed, but L1 (Memory) is operational");
            return true; // Still functional with L1
        }
    }
}
