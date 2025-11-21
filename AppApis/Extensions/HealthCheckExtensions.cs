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
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), 
                     tags: new[] { "ready" });

        var cacheProvider = configuration["Cache:Provider"];
        if (cacheProvider == "Redis" || cacheProvider == "Hybrid")
        {
            var redisConnection = configuration["Cache:Redis:ConnectionString"];
            if (!string.IsNullOrEmpty(redisConnection))
            {
                healthChecks.AddRedis(
                    redisConnection,
                    name: "redis",
                    tags: new[] { "cache", "redis", "ready" }
                );
            }
        }

        var dbConnection = configuration["ConnectionStrings:DefaultConnection"];
        if (!string.IsNullOrEmpty(dbConnection))
        {
            healthChecks.AddNpgSql(
                dbConnection,
                name: "database",
                tags: new[] { "db", "postgresql", "ready" }
            );
        }

        // Health Check UI Services
        services.AddHealthChecksUI(settings =>
        {
            settings.SetEvaluationTimeInSeconds(30);
            settings.MaximumHistoryEntriesPerEndpoint(50);
            settings.SetMinimumSecondsBetweenFailureNotifications(60);
            settings.AddHealthCheckEndpoint("API Health Check", "/health");
        })
        .AddInMemoryStorage();

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // Detailed health check with all services
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Ready check - for services that must be healthy
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Live check - basic liveness probe
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Health Check UI Dashboard
        app.MapHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-ui-api";
        });

        return app;
    }
}
