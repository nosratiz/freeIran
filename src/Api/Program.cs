using System.Threading.RateLimiting;
using Api.Configuration;
using Api.Constants;
using Api.Endpoints;
using Application.Constants;
using Application.DTOs;
using Application.Services;
using Application.Validators;
using FluentValidation;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

// ============================================
// BOOTSTRAP LOGGING (catch startup errors)
// ============================================
SerilogSetup.ConfigureBootstrapLogger();

try
{
    Log.Information(DisplayStrings.LogMessages.ApplicationStarting);

    var builder = WebApplication.CreateBuilder(args);

    // ============================================
    // SERILOG - Cost-optimized structured logging
    // ============================================
    builder.AddSerilog();

    // ============================================
    // CONFIGURATION - AWS Secrets Manager Integration
    // ============================================

    // Load secrets from AWS Secrets Manager (if not in development)
    if (!builder.Environment.IsDevelopment())
    {
        var secretsConfig = builder.Configuration
            .GetSection(AwsSecretsManagerSettings.SectionName)
            .Get<AwsSecretsManagerSettings>();

        if (secretsConfig is not null)
        {
            builder.Configuration.AddAwsSecretsManager(
                secretsConfig.SecretName,
                secretsConfig.Region);
        }
    }

// ============================================
// SERVICES REGISTRATION
// ============================================

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(ApiConstants.Swagger.DocumentName, new()
    {
        Title = ApiConstants.Swagger.Title,
        Version = ApiConstants.Swagger.Version,
        Description = ApiConstants.Swagger.Description
    });
});

// Add Output Caching
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(ApiConstants.CacheDefaults.BasePolicyExpirationMinutes)));
    options.AddPolicy(ApiConstants.Policies.DashboardStatsCache, policy => 
        policy.Expire(TimeSpan.FromMinutes(ApiConstants.CacheDefaults.DashboardStatsCacheMinutes)).Tag(ApiConstants.Policies.DashboardStatsCache));
});

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.AddFixedWindowLimiter(ApiConstants.Policies.FixedRateLimiter, limiterOptions =>
    {
        limiterOptions.PermitLimit = builder.Configuration.GetValue<int>(ApiConstants.ConfigurationKeys.PermitLimit, ApiConstants.RateLimitingDefaults.PermitLimit);
        limiterOptions.Window = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>(ApiConstants.ConfigurationKeys.WindowMinutes, ApiConstants.RateLimitingDefaults.WindowMinutes));
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = builder.Configuration.GetValue<int>(ApiConstants.ConfigurationKeys.QueueLimit, ApiConstants.RateLimitingDefaults.QueueLimit);
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = ApiConstants.ContentTypes.ApplicationJson;
        
        var response = new ApiErrorResponse
        {
            Code = ApiConstants.RateLimiting.ErrorCode,
            Message = ApiConstants.RateLimiting.ErrorMessage,
            Timestamp = DateTime.UtcNow
        };
        
        await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(ApiConstants.Policies.AllowConfiguredOrigins, policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection(ApiConstants.ConfigurationKeys.CorsAllowedOrigins)
            .Get<string[]>() ?? ApiConstants.CorsDefaults.AllowedOrigins;

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(ApiConstants.CacheDefaults.CorsPreflightMaxAgeMinutes));
    });
});

// Add Problem Details for consistent error responses
builder.Services.AddProblemDetails();

// Add DynamoDB
builder.Services.AddDynamoDb(builder.Configuration);

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// Add Application Services
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Add Health Checks
builder.Services.AddHealthChecks();

// ============================================
// APPLICATION PIPELINE
// ============================================

    var app = builder.Build();

    // Serilog request logging (cost-optimized: excludes health checks, enriches errors only)
    app.UseSerilogRequestLoggingWithCostOptimization(builder.Configuration);

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(ApiConstants.Swagger.EndpointPath, ApiConstants.Swagger.EndpointName);
            options.RoutePrefix = string.Empty;
        });
    }

    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append(ApiConstants.SecurityHeaders.ContentTypeOptions, ApiConstants.SecurityHeaders.ContentTypeOptionsValue);
        context.Response.Headers.Append(ApiConstants.SecurityHeaders.FrameOptions, ApiConstants.SecurityHeaders.FrameOptionsValue);
        context.Response.Headers.Append(ApiConstants.SecurityHeaders.XssProtection, ApiConstants.SecurityHeaders.XssProtectionValue);
        context.Response.Headers.Append(ApiConstants.SecurityHeaders.ReferrerPolicy, ApiConstants.SecurityHeaders.ReferrerPolicyValue);
        context.Response.Headers.Append(ApiConstants.SecurityHeaders.ContentSecurityPolicy, ApiConstants.SecurityHeaders.ContentSecurityPolicyValue);
        await next();
    });

    app.UseHttpsRedirection();
    app.UseCors(ApiConstants.Policies.AllowConfiguredOrigins);
    app.UseRateLimiter();
    app.UseOutputCache();

    // Map endpoints
    app.MapRegistrationEndpoints();
    app.MapDashboardEndpoints();

    // Health check endpoint
    app.MapHealthChecks(ApiConstants.Routes.Health)
        .WithTags(ApiConstants.Tags.Health)
        .WithOpenApi();

    // Run the application
    Log.Information(DisplayStrings.LogMessages.ApplicationStarted);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, DisplayStrings.LogMessages.ApplicationFatalError);
    throw;
}
finally
{
    Log.Information(DisplayStrings.LogMessages.ApplicationStopping);
    Log.CloseAndFlush();
}

// Make Program class visible to integration tests
public partial class Program { }
