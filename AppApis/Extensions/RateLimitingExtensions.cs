using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace AppApis.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            ConfigureFixedWindowLimiter(options);
            ConfigureSlidingWindowLimiter(options);
            ConfigureTokenBucketLimiter(options);
            ConfigureConcurrencyLimiter(options);
            ConfigurePerIpLimiter(options);
            ConfigureRejectionHandler(options);
        });

        return services;
    }

    private static void ConfigureFixedWindowLimiter(RateLimiterOptions options)
    {
        options.AddFixedWindowLimiter("fixed", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 10;
        });
    }

    private static void ConfigureSlidingWindowLimiter(RateLimiterOptions options)
    {
        options.AddSlidingWindowLimiter("sliding", opt =>
        {
            opt.PermitLimit = 50;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.SegmentsPerWindow = 6;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 5;
        });
    }

    private static void ConfigureTokenBucketLimiter(RateLimiterOptions options)
    {
        options.AddTokenBucketLimiter("token", opt =>
        {
            opt.TokenLimit = 100;
            opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
            opt.TokensPerPeriod = 50;
            opt.QueueLimit = 10;
        });
    }

    private static void ConfigureConcurrencyLimiter(RateLimiterOptions options)
    {
        options.AddConcurrencyLimiter("concurrency", opt =>
        {
            opt.PermitLimit = 50;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 20;
        });
    }

    private static void ConfigurePerIpLimiter(RateLimiterOptions options)
    {
        options.AddPolicy("perIp", context =>
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            });
        });
    }

    private static void ConfigureRejectionHandler(RateLimiterOptions options)
    {
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            Log.Warning("Rate limit exceeded! IP: {IP}, Path: {Path}",
                context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                context.HttpContext.Request.Path);

            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests. Please try again later.",
                retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? retryAfter.TotalSeconds
                    : 60
            }, cancellationToken);
        };
    }
}
