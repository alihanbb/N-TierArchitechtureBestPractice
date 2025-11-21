namespace AppService.Infrastructure.Caching;

public class InMemoryCacheService : ICacheService
{
    private readonly Dictionary<string, (object Value, DateTime? Expiration)> _cache = new();
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly object _lock = new();

    public InMemoryCacheService(ILogger<InMemoryCacheService> logger)
    {
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var cached))
            {
                if (cached.Expiration == null || cached.Expiration > DateTime.UtcNow)
                {
                    _logger.LogDebug("In-memory cache hit for key: {Key}", key);
                    return Task.FromResult((T?)cached.Value);
                }

                _cache.Remove(key);
            }

            _logger.LogDebug("In-memory cache miss for key: {Key}", key);
            return Task.FromResult(default(T));
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var expirationTime = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : (DateTime?)null;
            _cache[key] = (value!, expirationTime);
            _logger.LogDebug("In-memory cached key: {Key}, Expiration: {Expiration}", key, expiration);
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _cache.Remove(key);
            _logger.LogDebug("Removed in-memory cache key: {Key}", key);
        }
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var keysToRemove = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
            _logger.LogInformation("Removed {Count} in-memory keys with prefix: {Prefix}", keysToRemove.Count, prefix);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_cache.ContainsKey(key));
        }
    }

    public Task<bool> IsHealthyAsync()
    {
        return Task.FromResult(true);
    }
}
