using AppApis.Extensions;
using AppRepository.Extentions;
using AppService.Extentions;
using Serilog.Events;

// Bootstrap Logger - Initial logging before full configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Application starting...");

    var builder = WebApplication.CreateBuilder(args);

    // ===== Logging Configuration =====
    builder.Host.ConfigureSerilogLogging();

    // ===== Service Registration =====
    builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    // API Versioning
    builder.Services.AddApiVersioningConfiguration();

    // Rate Limiting
    builder.Services.AddRateLimitingConfiguration();

    // Hybrid Cache (L1 + L2)
    builder.Services.AddHybridCache(builder.Configuration);

    // Health Checks with UI
    builder.Services.AddHealthCheckConfiguration(builder.Configuration);

    // Application Services & Repository
    Log.Information("📦 Registering application services...");
    builder.Services.AddServices(builder.Configuration);

    if (!builder.Environment.IsEnvironment("Testing"))
    {
        Log.Information("💾 Registering repository layer...");
        builder.Services.AddRepository(builder.Configuration);
    }

    // ===== Middleware Pipeline =====
    var app = builder.Build();

    // Serilog Request Logging
    app.ConfigureSerilogRequestLogging();

    // Rate Limiting
    app.UseRateLimiter();

    // Development Tools
    if (app.Environment.IsDevelopment())
    {
        Log.Information("🔧 Development environment detected. Enabling Swagger...");
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Exception Handling
    app.UseExceptionHandler(_ => { });

    // Standard Middleware
    app.UseHttpsRedirection();
    app.UseAuthorization();

    // Health Check Endpoints (includes UI)
    app.MapHealthCheckEndpoints();

    // API Controllers
    app.MapControllers();

    Log.Information("✅ Application started successfully.");
    Log.Information("📊 Health Check UI available at: /health-ui");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application failed to start!");
    throw;
}
finally
{
    Log.Information("🛑 Application shutting down...");
    Log.CloseAndFlush();
}

// Required for integration tests
public partial class Program { }
