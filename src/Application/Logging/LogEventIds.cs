namespace Application.Logging;

/// <summary>
/// Event ID ranges for structured logging across the application.
/// Using defined ranges ensures log filtering and correlation work correctly.
/// 
/// COST OPTIMIZATION NOTES:
/// - Use LogLevel.Debug for verbose operational logs (filtered in production)
/// - Use LogLevel.Information sparingly for business-important events
/// - Use LogLevel.Warning for recoverable issues (rate limited, validation failed)
/// - Use LogLevel.Error for failures requiring attention
/// 
/// RANGES:
/// - 1000-1999: Application Services (Registration, etc.)
/// - 2000-2999: Application Services (Dashboard, etc.)
/// - 3000-3999: Infrastructure (Repository, DynamoDB)
/// - 4000-4999: API Layer (Endpoints, Middleware)
/// - 5000-5999: Security (Auth, Rate Limiting)
/// - 9000-9999: System (Startup, Shutdown, Health)
/// </summary>
public static class LogEventIds
{
    // Registration Service (1000-1099)
    public const int RegistrationValidationFailed = 1001;
    public const int RegistrationEmailCheckFailed = 1002;
    public const int RegistrationDuplicateEmail = 1003;
    public const int RegistrationCreateFailed = 1004;
    public const int RegistrationSuccess = 1005;
    public const int RegistrationRetrieveFailed = 1006;

    // Dashboard Service (2000-2099)
    public const int DashboardFetchingStats = 2001;
    public const int DashboardFetchFailed = 2002;
    public const int DashboardProcessing = 2003;
    public const int DashboardStatsGenerated = 2004;

    // DynamoDB Repository (3000-3099)
    public const int DynamoDbRegistrantCreated = 3001;
    public const int DynamoDbCreateError = 3002;
    public const int DynamoDbUnexpectedCreateError = 3003;
    public const int DynamoDbFetchError = 3004;
    public const int DynamoDbEmailFetchError = 3005;
    public const int DynamoDbRegistrantsRetrieved = 3006;
    public const int DynamoDbScanError = 3007;

    // API Layer (4000-4099)
    public const int ApiRequestReceived = 4001;
    public const int ApiRequestCompleted = 4002;
    public const int ApiValidationError = 4003;

    // Security (5000-5099)
    public const int RateLimitExceeded = 5001;
    public const int AuthenticationFailed = 5002;

    // System (9000-9099)
    public const int ApplicationStarting = 9001;
    public const int ApplicationStarted = 9002;
    public const int ApplicationStopping = 9003;
    public const int ApplicationStopped = 9004;
}

/// <summary>
/// Structured log property names for consistency across the application.
/// Use these constants to ensure property names are consistent for log aggregation.
/// </summary>
public static class LogProperties
{
    // Identifiers
    public const string RegistrantId = "RegistrantId";
    public const string Email = "Email";
    public const string CorrelationId = "CorrelationId";
    
    // Counts
    public const string ErrorCount = "ErrorCount";
    public const string RegistrantCount = "RegistrantCount";
    public const string TotalRegistrations = "TotalRegistrations";
    public const string CountryCount = "CountryCount";
    
    // Infrastructure
    public const string TableName = "TableName";
    public const string ErrorMessage = "ErrorMessage";
    
    // Performance
    public const string ElapsedMs = "ElapsedMs";
    public const string OperationName = "OperationName";
}
