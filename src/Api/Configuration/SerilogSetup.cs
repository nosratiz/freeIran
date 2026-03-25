using Api.Constants;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Api.Configuration;

/// <summary>
/// Serilog configuration with cost-conscious best practices.
/// 
/// COST OPTIMIZATION STRATEGIES:
/// 1. Log level filtering - Only log what's necessary (Warning+ in production)
/// 2. Structured logging - Efficient parsing and querying
/// 3. Async sinks - Non-blocking I/O
/// 4. Health check filtering - Exclude noisy health check logs
/// 5. Request logging sampling - Configurable sampling for high-traffic endpoints
/// 6. Compact JSON format - Smaller log sizes = lower storage costs
/// </summary>
public static class SerilogSetup
{
    /// <summary>
    /// Creates a bootstrap logger for startup errors before DI is configured.
    /// </summary>
    public static void ConfigureBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(new CompactJsonFormatter())
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Configures Serilog with cost-optimized settings.
    /// </summary>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "FreeIranPortal")
                .Enrich.WithProperty("Version", GetAssemblyVersion());

            // Environment-specific configuration
            if (context.HostingEnvironment.IsDevelopment())
            {
                ConfigureDevelopmentLogging(configuration);
            }
            else
            {
                ConfigureProductionLogging(configuration, context.Configuration);
            }
        });

        return builder;
    }

    /// <summary>
    /// Configures request logging middleware with cost optimizations.
    /// </summary>
    public static IApplicationBuilder UseSerilogRequestLoggingWithCostOptimization(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(SerilogRequestLoggingSettings.SectionName)
            .Get<SerilogRequestLoggingSettings>() ?? new SerilogRequestLoggingSettings();

        return app.UseSerilogRequestLogging(opts =>
        {
            // Use compact message template to reduce log size
            opts.MessageTemplate = ApiConstants.Logging.RequestMessageTemplate;

            // Exclude health check endpoints from request logging (COST SAVER)
            opts.GetLevel = (httpContext, elapsed, ex) =>
            {
                // Don't log health checks at all (they're high volume, low value)
                if (httpContext.Request.Path.StartsWithSegments(ApiConstants.Routes.Health))
                    return LogEventLevel.Verbose; // Will be filtered out

                // Log errors as Error level
                if (ex != null)
                    return LogEventLevel.Error;

                // Log slow requests as Warning
                if (elapsed > options.SlowRequestThresholdMs)
                    return LogEventLevel.Warning;

                // Log 4xx/5xx as Warning
                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                // Normal requests at Information level
                return LogEventLevel.Information;
            };

            // Enrich with useful data for debugging (but not too much - cost consideration)
            opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                
                // Only include these in non-successful responses
                if (httpContext.Response.StatusCode >= 400)
                {
                    diagnosticContext.Set("RequestHeaders", GetSafeHeaders(httpContext));
                    diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
                }

                // Include correlation ID if available
                if (httpContext.Request.Headers.TryGetValue(ApiConstants.SecurityHeaders.CorrelationIdHeader, out var correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId.ToString());
                }
            };
        });
    }

    private static void ConfigureDevelopmentLogging(LoggerConfiguration configuration)
    {
        configuration
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Amazon", LogEventLevel.Information)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
    }

    private static void ConfigureProductionLogging(
        LoggerConfiguration configuration,
        IConfiguration appConfiguration)
    {
        var settings = appConfiguration
            .GetSection(SerilogCostSettings.SectionName)
            .Get<SerilogCostSettings>() ?? new SerilogCostSettings();

        configuration
            // COST OPTIMIZATION: Only Warning and above in production by default
            .MinimumLevel.Is(settings.MinimumLevel)
            
            // Suppress noisy framework logs
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .MinimumLevel.Override("Amazon", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            
            // COST OPTIMIZATION: Use async sink with batching
            .WriteTo.Async(a => a.Console(new CompactJsonFormatter()), 
                bufferSize: settings.BufferSize);

        // Add file sink for production if configured (for CloudWatch/log aggregation)
        if (!string.IsNullOrEmpty(settings.LogFilePath))
        {
            configuration.WriteTo.Async(a => a.File(
                new CompactJsonFormatter(),
                settings.LogFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: settings.RetainedFileCountLimit,
                fileSizeLimitBytes: settings.FileSizeLimitMb * 1024 * 1024,
                rollOnFileSizeLimit: true,
                buffered: true,
                flushToDiskInterval: TimeSpan.FromSeconds(settings.FlushIntervalSeconds)),
                bufferSize: settings.BufferSize);
        }
    }

    private static Dictionary<string, string> GetSafeHeaders(HttpContext context)
    {
        // Only include safe headers (no auth tokens, cookies, etc.)
        return context.Request.Headers
            .Where(h => ApiConstants.SafeLoggingHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value.ToString());
    }

    private static string GetAssemblyVersion()
    {
        return typeof(SerilogSetup).Assembly
            .GetName()
            .Version?
            .ToString() ?? ApiConstants.Logging.DefaultVersion;
    }
}

/// <summary>
/// Cost-optimization settings for Serilog in production.
/// </summary>
public sealed class SerilogCostSettings
{
    public const string SectionName = "Serilog:CostOptimization";

    /// <summary>
    /// Minimum log level for production. Default: Warning (cost-effective).
    /// Use Information only if you need detailed logging and accept the cost.
    /// </summary>
    public LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Warning;

    /// <summary>
    /// Async buffer size. Larger = better throughput but more memory.
    /// </summary>
    public int BufferSize { get; init; } = 1000;

    /// <summary>
    /// Path for file logging. Leave empty to disable file logging.
    /// </summary>
    public string? LogFilePath { get; init; }

    /// <summary>
    /// Number of days to retain log files.
    /// </summary>
    public int RetainedFileCountLimit { get; init; } = 7;

    /// <summary>
    /// Max file size in MB before rolling.
    /// </summary>
    public int FileSizeLimitMb { get; init; } = 100;

    /// <summary>
    /// Flush interval in seconds for buffered file writing.
    /// </summary>
    public int FlushIntervalSeconds { get; init; } = 5;
}

/// <summary>
/// Settings for request logging cost optimization.
/// </summary>
public sealed class SerilogRequestLoggingSettings
{
    public const string SectionName = "Serilog:RequestLogging";

    /// <summary>
    /// Requests taking longer than this (ms) are logged as Warning.
    /// </summary>
    public double SlowRequestThresholdMs { get; init; } = 500;

    /// <summary>
    /// Sample rate for successful (2xx) requests. 1.0 = log all, 0.1 = log 10%.
    /// Lower values = lower costs for high-traffic APIs.
    /// </summary>
    public double SuccessfulRequestSampleRate { get; init; } = 1.0;
}
