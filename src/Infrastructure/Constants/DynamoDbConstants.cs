namespace Infrastructure.Constants;

/// <summary>
/// Centralized constants for DynamoDB configuration and operations.
/// </summary>
public static class DynamoDbConstants
{
    /// <summary>
    /// Table names.
    /// </summary>
    public static class Tables
    {
        public const string Registrants = "Registrants";
    }

    /// <summary>
    /// Index names for Global Secondary Indexes.
    /// </summary>
    public static class Indexes
    {
        public const string EmailIndex = "email-index";
        public const string CountryIndex = "country-index";
    }

    /// <summary>
    /// DynamoDB expression attribute names.
    /// </summary>
    public static class ExpressionAttributes
    {
        public const string EmailPlaceholder = ":email";
    }

    /// <summary>
    /// DynamoDB key expressions.
    /// </summary>
    public static class KeyExpressions
    {
        public const string EmailEquals = "Email = :email";
    }

    /// <summary>
    /// Date/time formats used for DynamoDB storage.
    /// </summary>
    public static class DateFormats
    {
        /// <summary>
        /// ISO 8601 date format (yyyy-MM-dd) for DateOnly values.
        /// </summary>
        public const string DateOnly = "yyyy-MM-dd";
        
        /// <summary>
        /// ISO 8601 round-trip format for DateTime values.
        /// </summary>
        public const string RoundTrip = "O";
    }

    /// <summary>
    /// AWS configuration section names.
    /// </summary>
    public static class ConfigurationSections
    {
        public const string DynamoDb = "DynamoDb";
    }

    /// <summary>
    /// AWS Secrets Manager configuration.
    /// </summary>
    public static class SecretsManager
    {
        public const string VersionStage = "AWSCURRENT";
    }

    /// <summary>
    /// Configuration keys for DynamoDB credentials.
    /// </summary>
    public static class ConfigurationKeys
    {
        public const string AccessKey = "DynamoDbAccessKey";
        public const string SecretKey = "DynamoDbSecretKey";
        public const string UseLocalSecrets = "UseLocalSecrets";
    }

    /// <summary>
    /// Error message templates for infrastructure operations.
    /// </summary>
    public static class ErrorMessages
    {
        // Configuration errors
        public const string MissingSettingsTemplate = "DynamoDB settings section '{0}' is missing or invalid.";
        public const string SecretNotFoundTemplate = "Secret '{0}' was not found in AWS Secrets Manager.";
        public const string InvalidRequestTemplate = "Invalid request to AWS Secrets Manager: {0}";
        public const string InvalidParameterTemplate = "Invalid parameter in Secrets Manager request: {0}";
        
        // Repository errors
        public const string DatabaseErrorTemplate = "Database error: {0}";
        public const string UnexpectedError = "An unexpected error occurred while saving the registrant.";
        public const string RegistrantNotFoundTemplate = "Registrant with ID '{0}' was not found.";
        
        public static string MissingSettings(string sectionName) => 
            string.Format(MissingSettingsTemplate, sectionName);
        
        public static string SecretNotFound(string secretName) => 
            string.Format(SecretNotFoundTemplate, secretName);
        
        public static string InvalidRequest(string message) => 
            string.Format(InvalidRequestTemplate, message);
        
        public static string InvalidParameter(string message) => 
            string.Format(InvalidParameterTemplate, message);
        
        public static string DatabaseError(string message) => 
            string.Format(DatabaseErrorTemplate, message);
        
        public static string RegistrantNotFound(string id) => 
            string.Format(RegistrantNotFoundTemplate, id);
    }
}
