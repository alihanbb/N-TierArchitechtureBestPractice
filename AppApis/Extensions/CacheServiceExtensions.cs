using AppService.Infrastructure.Caching;
using StackExchange.Redis;

namespace AppApis.Extensions;

public static class CacheServiceExtensions
{
    public static IServiceCollection AddHybridCache(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Memory Cache with size limits
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = configuration.GetValue<int?>("Cache:L1:MaxSize") ?? 1024;
            options.CompactionPercentage = 0.25;
        });

        var cacheProvider = configuration["Cache:Provider"];

        if (cacheProvider == "Redis" || cacheProvider == "Hybrid")
        {
            return AddHybridCacheWithRedis(services, configuration);
        }

        return AddInMemoryCacheOnly(services);
    }

    private static IServiceCollection AddHybridCacheWithRedis(IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var redisConnection = configuration["Cache:Redis:ConnectionString"];
            
            if (string.IsNullOrEmpty(redisConnection))
            {
                Log.Warning("Redis connection string is not configured, falling back to in-memory cache");
                return AddInMemoryCacheOnly(services);
            }

            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConnection));

            services.AddSingleton<ICacheService, HybridCacheDecorator>();
            
            Log.Information("✓ Hybrid Cache registered (L1: Memory + L2: Redis)");
            return services;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to connect to Redis, using in-memory cache only");
            return AddInMemoryCacheOnly(services);
        }
    }

    private static IServiceCollection AddInMemoryCacheOnly(IServiceCollection services)
    {
        services.AddSingleton<ICacheService, InMemoryCacheService>();
        Log.Information("✓ In-Memory Cache registered (L1 only)");
        return services;
    }
}
