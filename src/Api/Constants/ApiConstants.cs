namespace Api.Constants;

/// <summary>
/// Centralized constants for API configuration, policies, and security headers.
/// </summary>
public static class ApiConstants
{
    /// <summary>
    /// API metadata for OpenAPI/Swagger documentation.
    /// </summary>
    public static class Swagger
    {
        public const string Title = "Free Iran Reconstruction Portal API";
        public const string Version = "v1";
        public const string Description = "API for the Free Iran Reconstruction Portal - Registrant management and dashboard statistics.";
        public const string DocumentName = "v1";
        public const string EndpointPath = "/swagger/v1/swagger.json";
        public const string EndpointName = "Free Iran Portal API v1";
    }

    /// <summary>
    /// Policy names used throughout the application.
    /// </summary>
    public static class Policies
    {
        public const string FixedRateLimiter = "fixed";
        public const string DashboardStatsCache = "dashboard-stats";
        public const string AllowConfiguredOrigins = "AllowConfiguredOrigins";
    }

    /// <summary>
    /// API route configuration.
    /// </summary>
    public static class Routes
    {
        public const string ApiPrefix = "/api";
        public const string Register = "/register";
        public const string RegistrantsById = "/registrants/{id}";
        public const string Dashboard = "/dashboard";
        public const string DashboardStats = "/stats";
        public const string Health = "/health";
    }

    /// <summary>
    /// Tag names for endpoint grouping.
    /// </summary>
    public static class Tags
    {
        public const string Registration = "Registration";
        public const string Dashboard = "Dashboard";
        public const string Health = "Health";
    }

    /// <summary>
    /// Security-related HTTP headers.
    /// </summary>
    public static class SecurityHeaders
    {
        public const string ContentTypeOptions = "X-Content-Type-Options";
        public const string ContentTypeOptionsValue = "nosniff";
        
        public const string FrameOptions = "X-Frame-Options";
        public const string FrameOptionsValue = "DENY";
        
        public const string XssProtection = "X-XSS-Protection";
        public const string XssProtectionValue = "1; mode=block";
        
        public const string ReferrerPolicy = "Referrer-Policy";
        public const string ReferrerPolicyValue = "strict-origin-when-cross-origin";
        
        public const string ContentSecurityPolicy = "Content-Security-Policy";
        public const string ContentSecurityPolicyValue = "default-src 'self'";
        
        public const string CorrelationIdHeader = "X-Correlation-ID";
    }

    /// <summary>
    /// Safe headers to include in diagnostic logging.
    /// </summary>
    public static readonly string[] SafeLoggingHeaders =
    [
        "Content-Type",
        "Accept",
        "Accept-Language",
        "Origin",
        "Referer"
    ];

    /// <summary>
    /// Rate limiting error response codes and messages.
    /// </summary>
    public static class RateLimiting
    {
        public const string ErrorCode = "RATE_LIMIT_EXCEEDED";
        public const string ErrorMessage = "Too many requests. Please try again later.";
    }

    /// <summary>
    /// Default configuration values for rate limiting.
    /// </summary>
    public static class RateLimitingDefaults
    {
        public const int PermitLimit = 100;
        public const int WindowMinutes = 1;
        public const int QueueLimit = 10;
    }

    /// <summary>
    /// Default configuration values for caching.
    /// </summary>
    public static class CacheDefaults
    {
        public const int BasePolicyExpirationMinutes = 1;
        public const int DashboardStatsCacheMinutes = 5;
        public const int CorsPreflightMaxAgeMinutes = 10;
    }

    /// <summary>
    /// Default fallback values for CORS configuration.
    /// </summary>
    public static class CorsDefaults
    {
        public static readonly string[] AllowedOrigins = ["http://localhost:3000"];
    }

    /// <summary>
    /// Configuration section keys.
    /// </summary>
    public static class ConfigurationKeys
    {
        public const string RateLimitingSection = "RateLimiting";
        public const string PermitLimit = "RateLimiting:PermitLimit";
        public const string WindowMinutes = "RateLimiting:WindowMinutes";
        public const string QueueLimit = "RateLimiting:QueueLimit";
        public const string CorsAllowedOrigins = "Cors:AllowedOrigins";
    }

    /// <summary>
    /// Content types used in responses.
    /// </summary>
    public static class ContentTypes
    {
        public const string ApplicationJson = "application/json";
    }

    /// <summary>
    /// Default error codes for API responses.
    /// </summary>
    public static class ErrorCodes
    {
        public const string UnknownError = "UNKNOWN_ERROR";
        public const string ValidationError = "VALIDATION_ERROR";
    }

    /// <summary>
    /// Serilog request logging configuration.
    /// </summary>
    public static class Logging
    {
        public const string RequestMessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        public const string DefaultVersion = "1.0.0";
    }
}
