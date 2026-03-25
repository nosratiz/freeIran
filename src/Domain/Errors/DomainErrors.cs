namespace Domain.Errors;

/// <summary>
/// Domain-specific error codes for consistent error handling.
/// </summary>
public static class DomainErrors
{
    public static class Registrant
    {
        public const string EmailAlreadyExists = "REGISTRANT_EMAIL_EXISTS";
        public const string NotFound = "REGISTRANT_NOT_FOUND";
        public const string InvalidAge = "REGISTRANT_INVALID_AGE";
        public const string ValidationFailed = "REGISTRANT_VALIDATION_FAILED";
    }

    public static class Repository
    {
        public const string ConnectionFailed = "REPOSITORY_CONNECTION_FAILED";
        public const string OperationFailed = "REPOSITORY_OPERATION_FAILED";
        public const string Timeout = "REPOSITORY_TIMEOUT";
    }
}
