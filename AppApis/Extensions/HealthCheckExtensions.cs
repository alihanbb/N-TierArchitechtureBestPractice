using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

namespace AppApis.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthCheckConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"));

        var cacheProvider = configuration["Cache:Provider"];
        if (cacheProvider == "Redis" || cacheProvider == "Hybrid")
        {
            var redisConnection = configuration["Cache:Redis:ConnectionString"];
            if (!string.IsNullOrEmpty(redisConnection))
            {
                healthChecks.AddRedis(
                    redisConnection,
                    name: "redis",
                    tags: new[] { "cache", "redis" }
                );
            }
        }

        var dbConnection = configuration["ConnectionStrings:DefaultConnection"];
        if (!string.IsNullOrEmpty(dbConnection))
        {
            healthChecks.AddNpgSql(
                dbConnection,
                name: "database",
                tags: new[] { "db", "postgresql" }
            );
        }

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // Detailed health check with all services
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Ready check - for services that must be healthy
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Live check - basic liveness probe
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
